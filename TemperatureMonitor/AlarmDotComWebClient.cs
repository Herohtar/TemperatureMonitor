﻿using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TemperatureMonitor
{
    /// <summary>
    /// Alarm.com doesn't provide a public API. This class allows you to log in and obtain a valid session for making JSON requests
    /// </summary>
    public class AlarmDotComWebClient : WebClient
    {
        private string un;
        private string pw;

        private const string initialPageUrl = @"https://www.alarm.com/login.aspx";
        private const string loginFormUrl = @"https://www.alarm.com/web/Default.aspx";
        private const string temperatureSensorDataUrl = @"https://www.alarm.com/web/Dashboard/WebServices/Dashboard.asmx/TemperatureSensorDataRefresh";
        private const string userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:55.0) Gecko/20100101 Firefox/55.0"; // An actual user agent string so our request looks like it's from a real browser

        public AlarmDotComWebClient(string username, string password, CookieContainer container, String ajax)
        {
            CookieContainer = container;
            AjaxRequestHeader = ajax;
            un = username;
            pw = password;
        }

        public AlarmDotComWebClient(string username, string password)
          : this(username, password, new CookieContainer(), String.Empty)
        { }

        public void Login()
        {
            NameValueCollection loginData = new NameValueCollection();
            HtmlDocument pageHtml = new HtmlDocument();
            HttpWebRequest request;
            WebResponse response;

            // Load the first page in order to pull the ASP states/keys so our login request looks legit
            request = (HttpWebRequest)WebRequest.Create(initialPageUrl);
            request.Method = "GET";
            request.UserAgent = userAgent;
            response = request.GetResponse();

            // Parse the response and create the login headers
            pageHtml.Load(response.GetResponseStream());
            // We need all the hidden ASP.NET state/event values. Grab everything that starts with double underscores just to make sure we get everything
            pageHtml.DocumentNode.Descendants("input").Where(i => i.Id.StartsWith("__")).ToList().ForEach(i => loginData.Add(i.Id, i.GetAttributeValue("value", String.Empty)));
            loginData.Add("IsFromNewSite", "1"); // Not sure what this does exactly, but it seems necessary to include it
            loginData.Add("JavaScriptTest", "1"); // Lie and say we support JavaScript
            loginData.Add("ctl00$ContentPlaceHolder1$loginform$txtUserName", un); // Username
            loginData.Add("txtPassword", pw.ToString()); // Password

            // Set up the actual login
            request = (HttpWebRequest)WebRequest.Create(loginFormUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = userAgent;
            request.Referer = initialPageUrl;

            // Write the header
            string data = string.Join("&", loginData.Cast<string>().Select(key => $"{key}={loginData[key]}"));
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            request.ContentLength = buffer.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(buffer, 0, buffer.Length);
            requestStream.Close();

            request.CookieContainer = new CookieContainer();

            // Submit the login and parse the response
            response = request.GetResponse();
            pageHtml.Load(response.GetResponseStream());
            response.Close();

            // Steal the request key and cookies for ourselves
            AjaxRequestHeader = pageHtml.DocumentNode.Descendants("input").Where(i => i.GetAttributeValue("class", String.Empty).Equals("unique-form-key")).First().GetAttributeValue("value", String.Empty);
            CookieContainer = request.CookieContainer;
        }

        public List<TemperatureSensorsData> GetSensorData(int temperatureSensorPollFrequency)
        {
            string response = null;
            bool success = false;
            do
            {
                try
                {
                    response = UploadString(temperatureSensorDataUrl, String.Format("{{\"temperaturesensorPollFrequency\":{0}}}", temperatureSensorPollFrequency));
                    success = true;
                }
                catch (WebException e)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("{0}: Logging back in... {1}", DateTime.Now, e.Message));
                    Login();
                }
            } while (!success);

            RootObject root = JsonConvert.DeserializeObject<RootObject>(response);

            return root.d.responseObject.temperatureSensorsData;
        }

        public CookieContainer CookieContainer { get; private set; }

        public String AjaxRequestHeader { get; private set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            request.CookieContainer = CookieContainer;
            request.Headers.Add("AjaxRequestUniqueKey", AjaxRequestHeader);
            request.UserAgent = userAgent;
            request.ContentType = "application/json; charset=utf-8";
            return request;
        }
    }
}
