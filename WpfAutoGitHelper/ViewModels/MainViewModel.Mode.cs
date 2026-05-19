using System;
using System.Windows.Input;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Models;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        public const int EasyLogTabIndex = 3;
        public const int AdvancedLogTabIndex = 8;
        public const int AutoAdvancedTabIndex = 10;

        private UiModeKind _uiMode = UiModeKind.Easy;
        private int _easyTabIndex;
        private int _advancedTabIndex;

        public bool IsEasyMode
        {
            get => _uiMode == UiModeKind.Easy;
            set => SetUiMode(value ? UiModeKind.Easy : UiModeKind.Advanced);
        }

        public bool IsAdvancedMode
        {
            get => _uiMode == UiModeKind.Advanced;
            set => SetUiMode(value ? UiModeKind.Advanced : UiModeKind.Easy);
        }

        public bool IsAutoAdvancedMode
        {
            get => _uiMode == UiModeKind.AutoAdvanced;
            set => SetUiMode(value ? UiModeKind.AutoAdvanced : UiModeKind.Easy);
        }

        public bool IsAnyAdvancedMode => _uiMode != UiModeKind.Easy;

        public int EasyTabIndex
        {
            get => _easyTabIndex;
            set
            {
                if (_easyTabIndex == value)
                    return;
                _easyTabIndex = value;
                OnPropertyChanged();
            }
        }

        public int AdvancedTabIndex
        {
            get => _advancedTabIndex;
            set
            {
                if (_advancedTabIndex == value)
                    return;
                _advancedTabIndex = value;
                OnPropertyChanged();
            }
        }

        public int ActiveLogTabIndex => IsAnyAdvancedMode ? AdvancedLogTabIndex : EasyLogTabIndex;

        public ICommand ToggleUiModeCommand { get; private set; }

        private void InitUiMode()
        {
            _uiMode = ParseUiMode(_settings.UiMode);
            if (string.IsNullOrWhiteSpace(_settings.UiMode))
                _uiMode = ParseUiMode(_settings.DefaultUiMode);
            ToggleUiModeCommand = new RelayCommand(ToggleUiMode, () => !IsBusy);
            OnPropertyChanged(nameof(IsEasyMode));
            OnPropertyChanged(nameof(IsAdvancedMode));
            OnPropertyChanged(nameof(IsAutoAdvancedMode));
            OnPropertyChanged(nameof(IsAnyAdvancedMode));
            OnPropertyChanged(nameof(DefaultUiModeIsEasy));
            OnPropertyChanged(nameof(DefaultUiModeIsAdvanced));
            OnPropertyChanged(nameof(DefaultUiModeIsAutoAdvanced));
            NotifyAdvUiLabels();
        }

        private void ToggleUiMode()
        {
            if (_uiMode == UiModeKind.Easy)
                SetUiMode(UiModeKind.Advanced);
            else if (_uiMode == UiModeKind.Advanced)
                SetUiMode(UiModeKind.AutoAdvanced);
            else
                SetUiMode(UiModeKind.Easy);
        }

        public bool DefaultUiModeIsEasy
        {
            get => ParseUiMode(_settings.DefaultUiMode) == UiModeKind.Easy;
            set
            {
                if (value)
                    SetDefaultUiMode(UiModeKind.Easy);
            }
        }

        public bool DefaultUiModeIsAdvanced
        {
            get => ParseUiMode(_settings.DefaultUiMode) == UiModeKind.Advanced;
            set
            {
                if (value)
                    SetDefaultUiMode(UiModeKind.Advanced);
            }
        }

        public bool DefaultUiModeIsAutoAdvanced
        {
            get => ParseUiMode(_settings.DefaultUiMode) == UiModeKind.AutoAdvanced;
            set
            {
                if (value)
                    SetDefaultUiMode(UiModeKind.AutoAdvanced);
            }
        }

        private void SetDefaultUiMode(UiModeKind mode)
        {
            var text = UiModeToSettingsText(mode);
            if (string.Equals(_settings.DefaultUiMode, text, StringComparison.OrdinalIgnoreCase))
                return;

            _settings.DefaultUiMode = text;
            OnPropertyChanged(nameof(DefaultUiModeIsEasy));
            OnPropertyChanged(nameof(DefaultUiModeIsAdvanced));
            OnPropertyChanged(nameof(DefaultUiModeIsAutoAdvanced));
            PersistSettings();
        }

        private void SetUiMode(UiModeKind mode)
        {
            if (_uiMode == mode)
                return;

            _uiMode = mode;
            _settings.UiMode = UiModeToSettingsText(mode);
            OnPropertyChanged(nameof(IsEasyMode));
            OnPropertyChanged(nameof(IsAdvancedMode));
            OnPropertyChanged(nameof(IsAutoAdvancedMode));
            OnPropertyChanged(nameof(IsAnyAdvancedMode));
            OnPropertyChanged(nameof(ActiveLogTabIndex));

            if (mode == UiModeKind.Easy)
                SelectedTabIndex = 0;
            else
            {
                AdvancedTabIndex = mode == UiModeKind.AutoAdvanced ? AutoAdvancedTabIndex : 0;
                if (HasValidRepo)
                {
                    _ = RefreshChangedFilesAsync();
                    _ = RefreshRecentCommitsAsync();
                    _ = RefreshRemoteBranchesAsync();
                    _ = RefreshAutoActionPreviewAsync();
                }
            }

            NotifyAdvUiLabels();
            PersistSettings();
        }

        private static UiModeKind ParseUiMode(string value)
        {
            if (string.Equals(value, "auto-advanced", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "autoadvanced", StringComparison.OrdinalIgnoreCase))
                return UiModeKind.AutoAdvanced;

            return string.Equals(value, "advanced", StringComparison.OrdinalIgnoreCase)
                ? UiModeKind.Advanced
                : UiModeKind.Easy;
        }

        private static string UiModeToSettingsText(UiModeKind mode)
        {
            if (mode == UiModeKind.Advanced)
                return "advanced";
            if (mode == UiModeKind.AutoAdvanced)
                return "auto-advanced";
            return "easy";
        }
    }
}
