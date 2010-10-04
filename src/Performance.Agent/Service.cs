using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace Performance.Agent
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

        #endregion

        #region Constructors

        public Service()
        {
            InitializeComponent();

            // Instantiating these are crazy-slow and causes net start to report a timeout
            CpuPerformanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            RequestsPerSecondCounter = new PerformanceCounter("ASP.NET Applications", "Requests/Sec", "__Total__");
            MemoryPerformanceCounter = new PerformanceCounter("Memory", "Available MBytes");

            Status = new Dictionary<string, string>();

            ClickTimer = new Timer(1000);
            ClickTimer.Elapsed += new ElapsedEventHandler(ClickTimer_Elapsed);

            HttpListener = new HttpListener();
            HttpListener.Prefixes.Add("http://+:7812/");
        }

        #endregion

        #region Methods

        protected override void OnStart(string[] args)
        {
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
        }

        #endregion

        #region Event Handlers

        private void RequestCallback(IAsyncResult result)
        {
            try
            {
                HttpListenerContext context = HttpListener.EndGetContext(result);

                string callback = context.Request.QueryString["callback"];

                string output = "{";
                bool foo = false;
                foreach (string key in Status.Keys)
                {
                    output += String.Format("{2}\n\t{0} : '{1}'", key, Status[key], foo ? "," : "");
                    foo = true;
                }
                output += "\n}";

                output = String.Format("{0}({1})", callback, output);

                Encoding encoding = new UTF8Encoding(false); // Whenever I have UTF8 problems it's BOM's fault
                byte[] outputBytes = encoding.GetBytes(output);

                context.Response.OutputStream.Write(outputBytes, 0, outputBytes.Length);
                context.Response.Close();
            }
            finally
            {
                HttpListener.BeginGetContext(new AsyncCallback(RequestCallback), null);
            }
        }

        void ClickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Status["CpuUsage"] = CpuPerformanceCounter.NextValue().ToString();
            Status["RequestsPerSecond"] = RequestsPerSecondCounter.NextValue().ToString();
            Status["FreeMemory"] = MemoryPerformanceCounter.NextValue().ToString();
        }

        #endregion
    }
}
