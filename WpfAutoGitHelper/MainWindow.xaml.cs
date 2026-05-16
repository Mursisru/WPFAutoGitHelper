using System;
using System.Windows;
using System.Windows.Threading;
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
                _viewModel.LanguageChanged += OnViewModelLanguageChanged;

                RefreshTabHeaders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "WPF Auto Git Helper", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
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
            ReleasesTab.Header = _viewModel.Ui.TabReleases;
            IdentityTab.Header = _viewModel.Ui.TabIdentity;
            LogTab.Header = _viewModel.Ui.TabLog;
            SettingsTab.Header = _viewModel.Ui.TabSettings;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_viewModel != null)
                _viewModel.LanguageChanged -= OnViewModelLanguageChanged;
            base.OnClosed(e);
        }
    }
}
