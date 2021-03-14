using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Zokma.Libs;
using Zokma.Libs.Logging;

namespace LiveSounds
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Guid instanceId = Guid.Empty;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.instanceId = Guid.NewGuid();

            Log.Init(Pathfinder.ApplicationRoot.FindPathName("Logs", "LiveSounds.log"));
            Log.Information("OnStartup: InstanceId = {InstanceId}", instanceId.ToString());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("OnExit: InstanceId = {InstanceId}", instanceId.ToString());
            Log.Close();

            base.OnExit(e);
        }
    }
}
