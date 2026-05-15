using System.ComponentModel;
using System.Reflection;

namespace WpfAutoGitHelper.Localization
{
    /// <summary>Bindable localized strings; refreshes when <see cref="Loc.Language"/> changes.</summary>
    public sealed class LocalizedUi : INotifyPropertyChanged
    {
        public LocalizedUi() => Loc.LanguageChanged += OnLanguageChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public string AppTitle => Loc.Get("AppTitle");
        public string AppSubtitle => Loc.Get("AppSubtitle");
        public string SettingsHeader => Loc.Get("Settings_Header");
        public string SettingsLanguage => Loc.Get("Settings_Language");
        public string SettingsTheme => Loc.Get("Settings_Theme");
        public string ThemeLight => Loc.Get("Theme_Light");
        public string ThemeDark => Loc.Get("Theme_Dark");
        public string SettingsConfirmCommit => Loc.Get("Settings_ConfirmCommit");
        public string SettingsAutoRefresh => Loc.Get("Settings_AutoRefresh");
        public string BtnClearLog => Loc.Get("Btn_ClearLog");
        public string SettingsAutoSaveHint => Loc.Get("Settings_AutoSaveHint");
        public string RepoHeader => Loc.Get("Repo_Header");
        public string RepoHint => Loc.Get("Repo_Hint");
        public string RepoPathTip => Loc.Get("Repo_PathTip");
        public string RepoCurrentBranch => Loc.Get("Repo_CurrentBranch");
        public string BtnBrowse => Loc.Get("Btn_Browse");
        public string BtnSaveRepo => Loc.Get("Btn_SaveRepo");
        public string BtnOpenFolder => Loc.Get("Btn_OpenFolder");
        public string BtnOpenGitHub => Loc.Get("Btn_OpenGitHub");
        public string TipBrowse => Loc.Get("Tip_Browse");
        public string TipSaveRepo => Loc.Get("Tip_SaveRepo");
        public string TipOpenFolder => Loc.Get("Tip_OpenFolder");
        public string TipOpenGitHub => Loc.Get("Tip_OpenGitHub");
        public string ActionsHeader => Loc.Get("Actions_Header");
        public string ActionsHelpHeader => Loc.Get("Actions_HelpHeader");
        public string BtnPull => Loc.Get("Btn_Pull");
        public string BtnStatus => Loc.Get("Btn_Status");
        public string BtnDiff => Loc.Get("Btn_Diff");
        public string BtnAddAll => Loc.Get("Btn_AddAll");
        public string BtnCommit => Loc.Get("Btn_Commit");
        public string BtnPush => Loc.Get("Btn_Push");
        public string TipPull => Loc.Get("Tip_Pull");
        public string TipStatus => Loc.Get("Tip_Status");
        public string TipDiff => Loc.Get("Tip_Diff");
        public string TipAddAll => Loc.Get("Tip_AddAll");
        public string TipCommit => Loc.Get("Tip_Commit");
        public string TipPush => Loc.Get("Tip_Push");
        public string ActionsHelpText => string.Join(
            System.Environment.NewLine,
            Loc.Get("Help_Pull"),
            Loc.Get("Help_Status"),
            Loc.Get("Help_Diff"),
            Loc.Get("Help_AddAll"),
            Loc.Get("Help_Commit"),
            Loc.Get("Help_Push"));
        public string CommitHeader => Loc.Get("Commit_Header");
        public string BranchesHeader => Loc.Get("Branches_Header");
        public string BranchesNewTip => Loc.Get("Branches_NewTip");
        public string BranchesListTip => Loc.Get("Branches_ListTip");
        public string BtnCreateBranch => Loc.Get("Btn_CreateBranch");
        public string BtnCheckout => Loc.Get("Btn_Checkout");
        public string BtnPushBranch => Loc.Get("Btn_PushBranch");
        public string TipCreateBranch => Loc.Get("Tip_CreateBranch");
        public string TipCheckout => Loc.Get("Tip_Checkout");
        public string TipPushBranch => Loc.Get("Tip_PushBranch");
        public string IdentityHeader => Loc.Get("Identity_Header");
        public string IdentityUserName => Loc.Get("Identity_UserName");
        public string IdentityUserEmail => Loc.Get("Identity_UserEmail");
        public string IdentityLocalOnly => Loc.Get("Identity_LocalOnly");
        public string BtnLoadConfig => Loc.Get("Btn_LoadConfig");
        public string BtnApplyConfig => Loc.Get("Btn_ApplyConfig");
        public string TipLoadConfig => Loc.Get("Tip_LoadConfig");
        public string TipApplyConfig => Loc.Get("Tip_ApplyConfig");
        public string LogHeader => Loc.Get("Log_Header");
        public string TabActions => Loc.Get("Tab_Actions");
        public string TabReleases => Loc.Get("Tab_Releases");
        public string TabLog => Loc.Get("Tab_Log");
        public string TabSettings => Loc.Get("Tab_Settings");
        public string Step1Repo => Loc.Get("Step1_Repo");
        public string Step2Actions => Loc.Get("Step2_Actions");
        public string Step3Branches => Loc.Get("Step3_Branches");
        public string Step4Publish => Loc.Get("Step4_Publish");
        public string BranchesHelpHeader => Loc.Get("Branches_HelpHeader");
        public string BranchesExplain => Loc.Get("Branches_Explain");
        public string BtnCreateNewRepo => Loc.Get("Btn_CreateNewRepo");
        public string TipCreateNewRepo => Loc.Get("Tip_CreateNewRepo");
        public string PublishHint => Loc.Get("Publish_Hint");
        public string HelpWorkflowPrep => Loc.Get("Help_WorkflowPrep");
        public string TabIdentity => Loc.Get("Tab_Identity");
        public string IdentityGlobalHint => Loc.Get("Identity_GlobalHint");
        public string BtnClearIdentity => Loc.Get("Btn_ClearIdentity");
        public string TipClearIdentity => Loc.Get("Tip_ClearIdentity");
        public string BtnClearWorkflow => Loc.Get("Btn_ClearWorkflow");
        public string TipClearWorkflow => Loc.Get("Tip_ClearWorkflow");
        public string ReleasesHeader => Loc.Get("Releases_Header");
        public string ReleasesHint => Loc.Get("Releases_Hint");
        public string ReleaseTag => Loc.Get("Release_Tag");
        public string ReleaseTitle => Loc.Get("Release_Title");
        public string ReleaseNotes => Loc.Get("Release_Notes");
        public string ReleaseTarget => Loc.Get("Release_Target");
        public string ReleaseLatest => Loc.Get("Release_Latest");
        public string ReleasePrerelease => Loc.Get("Release_Prerelease");
        public string BtnCreateRelease => Loc.Get("Btn_CreateRelease");
        public string TipCreateRelease => Loc.Get("Tip_CreateRelease");
        public string BtnOpenReleases => Loc.Get("Btn_OpenReleases");
        public string TipOpenReleases => Loc.Get("Tip_OpenReleases");

        public void NotifyAllProperties()
        {
            foreach (var prop in typeof(LocalizedUi).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.PropertyType == typeof(string) && prop.CanRead)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop.Name));
            }
        }

        private void OnLanguageChanged() => NotifyAllProperties();
    }
}
