using System;
using System.Windows;
using System.Windows.Threading;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Services;
using WpfAutoGitHelper.Views;

namespace WpfAutoGitHelper
{
    public partial class App
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            try
            {
                var settings = SettingsStore.Load();
                AppearanceManager.Apply(
                    settings.Theme,
                    settings.AccentColor,
                    settings.BackgroundColor);

                var window = new MainWindow();
                window.Show();
            }
            catch (Exception ex)
            {
                ShowFatal(ex);
                Shutdown(1);
            }
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowFatal(e.Exception);
            e.Handled = true;
            Current?.Shutdown(1);
        }

        private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                ShowFatal(ex);
        }

        private static void ShowFatal(Exception ex)
        {
            try
            {
                var window = new FatalErrorWindow(ex.ToString());
                window.ShowDialog();
            }
            catch
            {
                // Last resort if themed window cannot load.
            }
        }
    }
}
