﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TemperatureMonitor.Utilities
{
    /// <summary>
    /// A collection of rules.
    /// </summary>
    /// <typeparam name="T">The type of the object the rules can be applied to.</typeparam>
    public sealed class RuleCollection<T> : Collection<Rule<T>>
    {
        /// <summary>
        /// Adds a new <see cref="Rule{T}"/> to this instance.
        /// </summary>
        /// <param name="propertyName">The name of the property the rules applies to.</param>
        /// <param name="error">The error if the object does not satisfy the rule.</param>
        /// <param name="rule">The rule to execute.</param>
        public void Add(string propertyName, object error, Func<T, bool> rule) =>
            Add(new DelegateRule<T>(propertyName, error, rule));

        /// <summary>
        /// Applies the <see cref="Rule{T}"/>'s contained in this instance to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The object to apply the rules to.</param>
        /// <param name="propertyName">Name of the property we want to apply rules for. <c>null</c>
        /// to apply all rules.</param>
        /// <returns>A collection of errors.</returns>
        public IEnumerable<object> Apply(T obj, string propertyName)
        {
            var errors = new List<object>();

            foreach (var rule in this)
            {
                if (string.IsNullOrEmpty(propertyName) || rule.PropertyName.Equals(propertyName))
                {
                    if (!rule.Apply(obj))
                    {
                        errors.Add(rule.Error);
                    }
                }
            }

            return errors;
        }
    }
}
