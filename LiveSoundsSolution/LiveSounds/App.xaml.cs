using LiveSounds.Localization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Zokma.Libs;
using Zokma.Libs.Logging;

namespace LiveSounds
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Checks main form loaded or not.
        /// </summary>
        internal static bool IsMainFormLoaded = false;

        /// <summary>
        /// Process instance id.
        /// </summary>
        private static readonly Guid processInstanceId;

        /// <summary>
        /// Application instance id.
        /// </summary>
        private Guid instanceId = Guid.Empty;

        static App()
        {
            processInstanceId = Guid.NewGuid();

            AppDomain.CurrentDomain.ProcessExit        += ProcessExitHandler;
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledExceptionHandler;
            TaskScheduler.UnobservedTaskException      += TaskSchedulerUnhandledExceptionHandler;
        }

        public App()
        {
            this.instanceId = Guid.NewGuid();

            //this.Exit += ProcessExitHandler;
            this.DispatcherUnhandledException += DispatcherUnhandledExceptionHandler;
        }

        /// <summary>
        /// Shows Error dialog for unexpected error.
        /// </summary>
        /// <param name="canContinue">true if the user can select continue option.</param>
        /// <returns>true if the user selected to be continued.</returns>
        private static bool ShowUnexpectedErrorDialog(bool canContinue = true)
        {
            string message = canContinue ? LocalizedInfo.UnexpectedErrorMessage : LocalizedInfo.UnexpectedErrorStopMessage;

            var button = canContinue ? MessageBoxButton.YesNo  : MessageBoxButton.OK;
            var icon   = canContinue ? MessageBoxImage.Warning : MessageBoxImage.Error;

            var result = MessageBox.Show(message, LocalizedInfo.UnexpectedErrorTitle, button, icon);

            return (result == MessageBoxResult.Yes);
        }

        /// <summary>
        /// Handler for UnhandledException.
        /// </summary>
        /// <param name="sender">sender of the event.</param>
        /// <param name="e">Event args.</param>
        private void DispatcherUnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "Unhandled Dispatcher Exception: InstanceId = {InstanceId}", this.instanceId);

            if (ShowUnexpectedErrorDialog(IsMainFormLoaded))
            {
                e.Handled = true;
            }
            else
            {
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Handler for UnhandledException.
        /// </summary>
        /// <param name="sender">sender of the event.</param>
        /// <param name="e">Event args.</param>
        private static void TaskSchedulerUnhandledExceptionHandler(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "Unhandled TaskScheduler Exception: ProcessInstanceId = {ProcessInstanceId}", processInstanceId);

            if (ShowUnexpectedErrorDialog(IsMainFormLoaded))
            {
                e.SetObserved();
            }
            else
            {
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Handler for UnhandledException.
        /// </summary>
        /// <param name="sender">sender of the event.</param>
        /// <param name="e">Event args.</param>
        private static void AppDomainUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.ExceptionObject as Exception, "Unhandled AppDomain Exception: ProcessInstanceId = {ProcessInstanceId}", processInstanceId);

            ShowUnexpectedErrorDialog(false);
            Environment.Exit(1);
        }

        /// <summary>
        /// Handler for ProcessExit.
        /// </summary>
        /// <param name="sender">sender of the event.</param>
        /// <param name="e">Event args.</param>
        private static void ProcessExitHandler(object sender, EventArgs e)
        {
            // In .NET Framework, "AppDomain.ProcessExit" timeouts after 2 secs.
            // But, In .NET Core there is no timeout.
            Log.Information("Process Exit: ProcessInstanceId = {ProcessInstanceId}", processInstanceId);
            Log.Close();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Log.Init(Pathfinder.ApplicationRoot.FindPathName("Logs", "LiveSounds.log"));
            Log.Information("OnStartup: InstanceId = {InstanceId}", this.instanceId.ToString());

            throw new Exception();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("OnExit: InstanceId = {InstanceId}", this.instanceId.ToString());

            base.OnExit(e);
        }
    }
}
