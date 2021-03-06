﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace RegardingYourPerformance.Agent
{
    public partial class Service : ServiceBase
    {
        #region Properties

        private HttpListener HttpListener
        {
            get;
            set;
        }

        private IDictionary<string, string> Status
        {
            get;
            set;
        }

        private PerformanceCounter CpuPerformanceCounter
        {
            get;
            set;
        }

        private PerformanceCounter MemoryPerformanceCounter
        {
            get;
            set;
        }

        private PerformanceCounter RequestsPerSecondCounter
        {
            get;
            set;
        }

        private Timer ClickTimer
        {
            get;
            set;
        }

        private float TotalPhysicalMemory
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public Service()
        {
            InitializeComponent();

            Status = new Dictionary<string, string>();

            double updateInterval = Convert.ToDouble(ConfigurationManager.AppSettings["UpdateInterval"]);
            ClickTimer = new Timer(updateInterval);
            ClickTimer.Elapsed += new ElapsedEventHandler(ClickTimer_Elapsed);

            string prefix = String.Format("http://+:{0}/", ConfigurationManager.AppSettings["ServerPort"]);
            HttpListener = new HttpListener();
            HttpListener.Prefixes.Add(prefix);
        }

        #endregion

        #region Methods

        protected override void OnStart(string[] args)
        {
            // Get total memory
            ManagementObjectSearcher wmi = new ManagementObjectSearcher("select * from Win32_ComputerSystem");
            foreach (ManagementObject o in wmi.Get())
                TotalPhysicalMemory += Convert.ToSingle(o["TotalPhysicalMemory"]) / (1024 * 1024);

            // Would rather have these in the constructor, but instantiating them is crazy-slow and 
            // causes net start to report a timeout
            CpuPerformanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            MemoryPerformanceCounter = new PerformanceCounter("Memory", "Available MBytes");
            RequestsPerSecondCounter = new PerformanceCounter("ASP.NET Applications", "Requests/Sec", "__Total__");

            // Since requests can arrive before these are first populated, fill with data so we don't
            // send junk to the client
            Status["CpuUsage"] = "0";
            Status["MemoryUsage"] = "0";
            Status["RequestsPerSecond"] = "0";

            ClickTimer.Start();
            HttpListener.Start();

            // 10 worker threads
            for (int i = 0; i < 10; i ++)
                HttpListener.BeginGetContext(new AsyncCallback(RequestCallback), null);
        }

        protected override void OnStop()
        {
            ClickTimer.Stop();
            ClickTimer.Dispose();
            HttpListener.Close();

            CpuPerformanceCounter.Dispose();
            MemoryPerformanceCounter.Dispose();
            RequestsPerSecondCounter.Dispose();
        }

        #endregion

        #region Event Handlers

        private void RequestCallback(IAsyncResult result)
        {
            HttpListenerContext context = null;
            try
            {
                context = HttpListener.EndGetContext(result);

                string callback = context.Request.QueryString["callback"] ?? context.Request.QueryString["jsonp"];

                string output = "{";
                bool foo = false;
                lock (Status)
                {
                    foreach (string key in Status.Keys)
                    {
                        output += String.Format("{2}\n\t{0} : '{1}'", key, Status[key], foo ? "," : "");
                        foo = true;
                    }
                }
                output += "\n}";

                if (!String.IsNullOrEmpty(callback))
                    output = String.Format("{0}({1})", callback, output);

                Encoding encoding = new UTF8Encoding(false); // Whenever I have UTF8 problems it's BOM's fault
                byte[] outputBytes = encoding.GetBytes(output);

                context.Response.OutputStream.Write(outputBytes, 0, outputBytes.Length);
            }
            finally
            {
                if (context != null && context.Response != null)
                    context.Response.Close();
                HttpListener.BeginGetContext(new AsyncCallback(RequestCallback), null);
            }
        }

        void ClickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (Status)
            {
                Status["CpuUsage"] = CpuPerformanceCounter.NextValue().ToString();
                Status["MemoryUsage"] = ((TotalPhysicalMemory - MemoryPerformanceCounter.NextValue()) * 100 / TotalPhysicalMemory).ToString();
                Status["RequestsPerSecond"] = RequestsPerSecondCounter.NextValue().ToString();
            }
        }

        #endregion
    }
}
