using Hit.GutService.Video;
using Hit.LoggerLibrary;
using Microsoft.Owin.Hosting;
using System;
using System.Configuration;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;

namespace Hit.GutService
{
    partial class WinGutService : ServiceBase
    {
        public string baseAddress = ConfigurationManager.AppSettings.Get("ServiceUrl");
        private IDisposable _server = null;

        public WinGutService()
        {
            ServiceName = "Hit.GutService";
        }
        public static void Main()
        {
            Run(new WinGutService());
        }
        protected override void OnStart(string[] args)
        {
            _server = WebApp.Start<Startup>(url: baseAddress);
            Logger.INFO(MethodBase.GetCurrentMethod(), "Server running on " + baseAddress);
        }
        protected override void OnStop()
        {
            if (_server != null)
            {
                Service.Stop();
                _server.Dispose();
            }
            base.OnStop();
        }
    }
}
