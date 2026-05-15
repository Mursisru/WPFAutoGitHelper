using System;
using System.Windows;
using System.Windows.Threading;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Services;

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
                ThemeManager.Apply(settings.Theme);

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
            MessageBox.Show(
                ex.ToString(),
                "WPF Auto Git Helper — startup error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
