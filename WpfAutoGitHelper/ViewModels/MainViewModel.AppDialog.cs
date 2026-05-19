using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfAutoGitHelper.Localization;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        public const int LogTabIndex = EasyLogTabIndex;

        private TaskCompletionSource<bool> _dialogTcs;
        private bool _isDialogOpen;
        private string _dialogTitle = "";
        private string _dialogMessage = "";
        private string _dialogInputText = "";
        private AppDialogMode _dialogMode;
        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex == value)
                    return;
                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }

        public bool IsDialogOpen
        {
            get => _isDialogOpen;
            private set
            {
                if (_isDialogOpen == value)
                    return;
                _isDialogOpen = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string DialogTitle
        {
            get => _dialogTitle;
            private set
            {
                if (_dialogTitle == value)
                    return;
                _dialogTitle = value ?? "";
                OnPropertyChanged();
            }
        }

        public string DialogMessage
        {
            get => _dialogMessage;
            private set
            {
                if (_dialogMessage == value)
                    return;
                _dialogMessage = value ?? "";
                OnPropertyChanged();
            }
        }

        public string DialogInputText
        {
            get => _dialogInputText;
            set
            {
                if (_dialogInputText == value)
                    return;
                _dialogInputText = value ?? "";
                OnPropertyChanged();
            }
        }

        public bool DialogShowInput => _dialogMode == AppDialogMode.InputOkCancel;

        public bool DialogShowYesNo => _dialogMode == AppDialogMode.YesNo;

        public bool DialogShowOkOnly => _dialogMode == AppDialogMode.Ok;

        public ICommand DialogOkCommand { get; private set; }
        public ICommand DialogCancelCommand { get; private set; }
        public ICommand DialogYesCommand { get; private set; }
        public ICommand DialogNoCommand { get; private set; }

        private void InitAppDialogCommands()
        {
            DialogOkCommand = new RelayCommand(() => CompleteDialog(true), () => IsDialogOpen);
            DialogCancelCommand = new RelayCommand(() => CompleteDialog(false), () => IsDialogOpen);
            DialogYesCommand = new RelayCommand(() => CompleteDialog(true), () => IsDialogOpen);
            DialogNoCommand = new RelayCommand(() => CompleteDialog(false), () => IsDialogOpen);
        }

        private void CompleteDialog(bool ok)
        {
            if (!IsDialogOpen)
                return;

            IsDialogOpen = false;
            _dialogTcs?.TrySetResult(ok);
        }

        private async Task<bool> ShowAppDialogAsync(string message, string title, AppDialogMode mode, string defaultInput = null)
        {
            await RunOnUiAsync(() =>
            {
                _dialogTcs = new TaskCompletionSource<bool>();
                DialogTitle = title ?? Caption;
                DialogMessage = message ?? "";
                _dialogMode = mode;
                DialogInputText = defaultInput ?? "";
                OnPropertyChanged(nameof(DialogShowInput));
                OnPropertyChanged(nameof(DialogShowYesNo));
                OnPropertyChanged(nameof(DialogShowOkOnly));
                IsDialogOpen = true;
            }).ConfigureAwait(true);

            return await _dialogTcs.Task.ConfigureAwait(true);
        }

        private Task RunOnUiAsync(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
                return Task.CompletedTask;
            }

            return dispatcher.InvokeAsync(action, DispatcherPriority.Normal).Task;
        }

        private async Task NotifyAsync(string message, string title = null, bool isError = false)
        {
            AppendLog(message, isError);
            if (IsAdvancedMode)
                AdvancedTabIndex = AdvancedLogTabIndex;
            else
                SelectedTabIndex = EasyLogTabIndex;
            await ShowAppDialogAsync(message, title ?? Caption, AppDialogMode.Ok).ConfigureAwait(true);
        }

        private Task<bool> ConfirmAsync(string message, string title = null) =>
            ShowAppDialogAsync(message, title ?? Caption, AppDialogMode.YesNo);

        private async Task<string> PromptInputAsync(string message, string title, string defaultValue)
        {
            if (!await ShowAppDialogAsync(message, title, AppDialogMode.InputOkCancel, defaultValue).ConfigureAwait(true))
                return null;

            var text = DialogInputText?.Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
    }
}
