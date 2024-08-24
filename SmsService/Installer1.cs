using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace SmsService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        private ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            InitializeComponent();

            serviceProcessInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            // Service will run under system account
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;

            //Service Information
            serviceInstaller.DisplayName = "SMS Windows Service";
            serviceInstaller.StartType = ServiceStartMode.Manual;

            // This must be identical to the WindowsService.ServiceBase name
            // set in the constructor of WindowsService.cs

            serviceInstaller.ServiceName = "SmsService";

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);

        }
    }
}
