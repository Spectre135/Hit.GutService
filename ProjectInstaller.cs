using System.ComponentModel;
using System.ServiceProcess;

namespace Hit.GutService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        public ProjectInstaller()
        {
            process = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };
            service = new ServiceInstaller
            {
                ServiceName = "Hit.GutService",
                Description = "Hit Windows Service za Geutebruck video system"
            };
            Installers.Add(process);
            Installers.Add(service);
        }
    }
}
