using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WpfAutoGitHelper.ViewModels;
using WpfAutoGitHelper.Views;

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
                Content = BuildStartupErrorContent(ex.ToString());
                Show();
                return;
            }
        }

        private void OnViewModelLanguageChanged()
        {
            Dispatcher.BeginInvoke((Action)RefreshTabHeaders, DispatcherPriority.DataBind);
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F7 || _viewModel == null)
                return;

            if (_viewModel.ToggleUiModeCommand?.CanExecute(null) == true)
                _viewModel.ToggleUiModeCommand.Execute(null);
            e.Handled = true;
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

        private static UIElement BuildStartupErrorContent(string errorText)
        {
            var grid = new Grid { Margin = new Thickness(16) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var title = new TextBlock
            {
                Style = (Style)Application.Current.FindResource("HeaderTitleStyle"),
                Text = "WPF Auto Git Helper",
                Margin = new Thickness(0, 0, 0, 10),
            };
            Grid.SetRow(title, 0);
            grid.Children.Add(title);

            var log = new TextBox
            {
                Style = (Style)Application.Current.FindResource("LogTextBoxStyle"),
                Text = errorText,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            Grid.SetRow(log, 1);
            grid.Children.Add(log);

            var close = new Button
            {
                Style = (Style)Application.Current.FindResource("PrimaryButtonStyle"),
                Content = "OK",
                MinWidth = 96,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0),
                IsDefault = true,
            };
            close.Click += (_, __) => Application.Current.Shutdown(1);
            Grid.SetRow(close, 2);
            grid.Children.Add(close);

            return grid;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_viewModel != null)
                _viewModel.LanguageChanged -= OnViewModelLanguageChanged;
            base.OnClosed(e);
        }
    }
}
