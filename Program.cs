#region using
using Hit.LoggerLibrary;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.ServiceProcess;
using System.Web.Http;
using System.Web.Http.Cors;
#endregion

[assembly: OwinStartup(typeof(Hit.GutService.Startup))]

namespace Hit.GutService
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = ConfigurationManager.AppSettings.Get("ServiceUrl");
            
            using (WebApp.Start<Startup>(url : baseAddress))
            {
                Console.WriteLine("Server running on " + baseAddress);
                Console.ReadLine();
            }
            
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

            //Hit auth filter
           config.Filters.Add(new System.Web.Http.AuthorizeAttribute());
            
            // Web API routes
            config.MapHttpAttributeRoutes();
            app.UseWebApi(config);

            app.UseCors(CorsOptions.AllowAll);
            GlobalHost.Configuration.DefaultMessageBufferSize = 5000;
            app.MapSignalR();

        }
    }

}
