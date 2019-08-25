using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace TemperatureMonitor.Utilities
{
    /// <summary>
    /// Provides functionality to provide errors for the object if it is in an invalid state.
    /// </summary>
    /// <typeparam name="T">The type of this instance.</typeparam>
    public abstract class NotifyDataErrorInfo<T> : NotifyPropertyChanges, INotifyDataErrorInfo
        where T : NotifyDataErrorInfo<T>
    {
        #region Fields

        private const string HasErrorsPropertyName = "HasErrors";
        private Dictionary<string, List<object>> errors;

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire object. 
        /// </summary>
        event EventHandler<DataErrorsChangedEventArgs> INotifyDataErrorInfo.ErrorsChanged
        {
            add { errorsChanged += value; }
            remove { errorsChanged -= value; }
        }

        #endregion

        #region Private Events

        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire object. 
        /// </summary>
        private event EventHandler<DataErrorsChangedEventArgs> errorsChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the when errors changed observable event. Occurs when the validation errors have changed for a property or for the entire object. 
        /// </summary>
        /// <value>
        /// The when errors changed observable event.
        /// </value>
        public IObservable<string> WhenErrorsChanged => Observable
                    .FromEventPattern<DataErrorsChangedEventArgs>(
                        h => errorsChanged += h,
                        h => errorsChanged -= h)
                    .Select(x => x.EventArgs.PropertyName);

        /// <summary>
        /// Gets a value indicating whether the object has validation errors. 
        /// </summary>
        /// <value><c>true</c> if this instance has errors, otherwise <c>false</c>.</value>
        public virtual bool HasErrors
        {
            get
            {
                InitializeErrors();
                return errors.Count > 0;
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the rules which provide the errors.
        /// </summary>
        /// <value>The rules this instance must satisfy.</value>
        protected static RuleCollection<T> Rules { get; } = new RuleCollection<T>();

        /// <summary>
        /// Gets the validation errors for the entire object.
        /// </summary>
        /// <returns>A collection of errors.</returns>
        public IEnumerable GetErrors()
        {
            return GetErrors(null);
        }

        /// <summary>
        /// Gets the validation errors for a specified property or for the entire object.
        /// </summary>
        /// <param name="propertyName">Name of the property to retrieve errors for. <c>null</c> to 
        /// retrieve all errors for this instance.</param>
        /// <returns>A collection of errors.</returns>
        public IEnumerable GetErrors(string propertyName)
        {
            InitializeErrors();

            IEnumerable result;
            if (string.IsNullOrEmpty(propertyName))
            {
                var allErrors = new List<object>();

                foreach (var keyValuePair in errors)
                {
                    allErrors.AddRange(keyValuePair.Value);
                }

                result = allErrors;
            }
            else
            {
                if (errors.ContainsKey(propertyName))
                {
                    result = errors[propertyName];
                }
                else
                {
                    result = new List<object>();
                }
            }

            return result;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (string.IsNullOrEmpty(propertyName))
            {
                ApplyRules();
            }
            else
            {
                ApplyRules(propertyName);
            }

            base.OnPropertyChanged(HasErrorsPropertyName);
        }

        /// <summary>
        /// Called when the errors have changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnErrorsChanged([CallerMemberName] string propertyName = null)
        {
            errorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Applies all rules to this instance.
        /// </summary>
        private void ApplyRules()
        {
            InitializeErrors();

            foreach (var propertyName in Rules.Select(x => x.PropertyName))
            {
                ApplyRules(propertyName);
            }
        }

        /// <summary>
        /// Applies the rules to this instance for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void ApplyRules(string propertyName)
        {
            InitializeErrors();

            var propertyErrors = Rules.Apply((T)this, propertyName).ToList();

            if (propertyErrors.Count > 0)
            {
                if (errors.ContainsKey(propertyName))
                {
                    errors[propertyName].Clear();
                }
                else
                {
                    errors[propertyName] = new List<object>();
                }

                errors[propertyName].AddRange(propertyErrors);
                OnErrorsChanged(propertyName);
            }
            else if (errors.ContainsKey(propertyName))
            {
                errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }

        /// <summary>
        /// Initializes the errors and applies the rules if not initialized.
        /// </summary>
        private void InitializeErrors()
        {
            if (errors == null)
            {
                errors = new Dictionary<string, List<object>>();

                ApplyRules();
            }
        }

        #endregion
    }
}
