using System;
using System.Windows;
using System.Windows.Threading;
using WpfAutoGitHelper.Services;
using WpfAutoGitHelper.ViewModels;

namespace WpfAutoGitHelper
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                _viewModel = new MainViewModel();
                DataContext = _viewModel;
                ThemeManager.ApplyTo(this, _viewModel.SelectedThemeId);
                ThemeManager.ThemeChanged += OnThemeChanged;
                _viewModel.LanguageChanged += OnViewModelLanguageChanged;

                RefreshTabHeaders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "WPF Auto Git Helper", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void OnThemeChanged()
        {
            if (_viewModel != null)
                ThemeManager.ApplyTo(this, _viewModel.SelectedThemeId);
        }

        private void OnViewModelLanguageChanged()
        {
            Dispatcher.BeginInvoke((Action)RefreshTabHeaders, DispatcherPriority.DataBind);
        }

        private void RefreshTabHeaders()
        {
            if (_viewModel == null)
                return;

            Title = _viewModel.Ui.AppTitle;
            ActionsTab.Header = _viewModel.Ui.TabActions;
            IdentityTab.Header = _viewModel.Ui.TabIdentity;
            ReleasesTab.Header = _viewModel.Ui.TabReleases;
            LogTab.Header = _viewModel.Ui.TabLog;
            SettingsTab.Header = _viewModel.Ui.TabSettings;
        }

        protected override void OnClosed(System.EventArgs e)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
            if (_viewModel != null)
                _viewModel.LanguageChanged -= OnViewModelLanguageChanged;
            base.OnClosed(e);
        }
    }
}
