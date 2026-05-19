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
        public string SettingsAccentColor => Loc.Get("Settings_AccentColor");
        public string SettingsBackgroundColor => Loc.Get("Settings_BackgroundColor");
        public string ThemeLight => Loc.Get("Theme_Light");
        public string ThemeDark => Loc.Get("Theme_Dark");
        public string ThemeBlack => Loc.Get("Theme_Black");
        public string SettingsConfirmCommit => Loc.Get("Settings_ConfirmCommit");
        public string SettingsShowFieldHints => Loc.Get("Settings_ShowFieldHints");
        public string SettingsAutoRefresh => Loc.Get("Settings_AutoRefresh");
        public string BtnClearLog => Loc.Get("Btn_ClearLog");
        public string BtnDialogOk => Loc.Get("Btn_DialogOk");
        public string BtnDialogCancel => Loc.Get("Btn_DialogCancel");
        public string BtnDialogYes => Loc.Get("Btn_DialogYes");
        public string BtnDialogNo => Loc.Get("Btn_DialogNo");
        public string SettingsAutoSaveHint => Loc.Get("Settings_AutoSaveHint");
        public string RepoHeader => Loc.Get("Repo_Header");
        public string RepoHint => Loc.Get("Repo_Hint");
        public string RepoPathTip => Loc.Get("Repo_PathTip");
        public string RepoCurrentBranch => Loc.Get("Repo_CurrentBranch");
        public string RepoSelectedProject => Loc.Get("Repo_SelectedProject");
        public string RepoSelectedFolder => Loc.Get("Repo_SelectedFolder");
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
        public string BtnSyncGitHub => Loc.Get("Btn_SyncGitHub");
        public string BtnConfigureOrigin => Loc.Get("Btn_ConfigureOrigin");
        public string TipPull => Loc.Get("Tip_Pull");
        public string TipStatus => Loc.Get("Tip_Status");
        public string TipDiff => Loc.Get("Tip_Diff");
        public string TipAddAll => Loc.Get("Tip_AddAll");
        public string TipCommit => Loc.Get("Tip_Commit");
        public string TipPush => Loc.Get("Tip_Push");
        public string TipSyncGitHub => Loc.Get("Tip_SyncGitHub");
        public string TipConfigureOrigin => Loc.Get("Tip_ConfigureOrigin");
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
        public string ReleaseAssets => Loc.Get("Release_Assets");
        public string ReleaseAssetsHint => Loc.Get("Release_AssetsHint");
        public string BtnReleaseAddFiles => Loc.Get("Btn_ReleaseAddFiles");
        public string TipReleaseAddFiles => Loc.Get("Tip_ReleaseAddFiles");
        public string BtnReleaseAddImages => Loc.Get("Btn_ReleaseAddImages");
        public string TipReleaseAddImages => Loc.Get("Tip_ReleaseAddImages");
        public string BtnReleaseAddBuild => Loc.Get("Btn_ReleaseAddBuild");
        public string TipReleaseAddBuild => Loc.Get("Tip_ReleaseAddBuild");
        public string BtnReleaseRemoveFile => Loc.Get("Btn_ReleaseRemoveFile");
        public string TipReleaseRemoveFile => Loc.Get("Tip_ReleaseRemoveFile");
        public string TipReleaseAssetsList => Loc.Get("Tip_ReleaseAssetsList");
        public string BtnCreateRelease => Loc.Get("Btn_CreateRelease");
        public string TipCreateRelease => Loc.Get("Tip_CreateRelease");
        public string BtnOpenReleases => Loc.Get("Btn_OpenReleases");
        public string TipOpenReleases => Loc.Get("Tip_OpenReleases");

        public string UiModeEasy => Loc.Get("UiMode_Easy");
        public string UiModeAdvanced => Loc.Get("UiMode_Advanced");
        public string UiModeAutoAdvanced => Loc.Get("UiMode_AutoAdvanced");
        public string UiModeToggleTip => Loc.Get("UiMode_ToggleTip");
        public string SettingsDefaultUiMode => Loc.Get("Settings_DefaultUiMode");
        public string TabWorkflow => Loc.Get("Tab_Workflow");
        public string TabStaging => Loc.Get("Tab_Staging");
        public string TabPushAdv => Loc.Get("Tab_PushAdv");
        public string TabBranchesAdv => Loc.Get("Tab_BranchesAdv");
        public string TabFiles => Loc.Get("Tab_Files");
        public string TabSafety => Loc.Get("Tab_Safety");
        public string TabAdvancedHelp => Loc.Get("Tab_AdvancedHelp");
        public string FilesChangedHeader => Loc.Get("Files_ChangedHeader");
        public string BtnRestoreFile => Loc.Get("Btn_RestoreFile");
        public string BtnStageSelected => Loc.Get("Btn_StageSelected");
        public string BtnUnstageSelected => Loc.Get("Btn_UnstageSelected");
        public string BtnCommitSelectedPush => Loc.Get("Btn_CommitSelectedPush");
        public string BtnAmendCommit => Loc.Get("Btn_AmendCommit");
        public string BtnRevertCommit => Loc.Get("Btn_RevertCommit");
        public string BtnForcePushLease => Loc.Get("Btn_ForcePushLease");
        public string BtnForcePush => Loc.Get("Btn_ForcePush");
        public string BtnDraftBranch => Loc.Get("Btn_DraftBranch");
        public string BtnDeleteLocalBranch => Loc.Get("Btn_DeleteLocalBranch");
        public string BtnDeleteRemoteBranch => Loc.Get("Btn_DeleteRemoteBranch");
        public string BtnPruneRemote => Loc.Get("Btn_PruneRemote");
        public string BtnMergeBranch => Loc.Get("Btn_MergeBranch");
        public string BtnRebaseBranch => Loc.Get("Btn_RebaseBranch");
        public string BtnMergeContinue => Loc.Get("Btn_MergeContinue");
        public string BtnMergeAbort => Loc.Get("Btn_MergeAbort");
        public string BtnResolveConflicts => Loc.Get("Btn_ResolveConflicts");
        public string BtnGitRm => Loc.Get("Btn_GitRm");
        public string BtnDeleteUntracked => Loc.Get("Btn_DeleteUntracked");
        public string StagingSelectAll => Loc.Get("Staging_SelectAll");
        public string StagingSelectNone => Loc.Get("Staging_SelectNone");
        public string LabelMergeSource => Loc.Get("Label_MergeSource");
        public string LabelDraftBranchName => Loc.Get("Label_DraftBranchName");
        public string LabelRevertHash => Loc.Get("Label_RevertHash");
        public string TabModeHelp => Loc.Get("Tab_ModeHelp");
        public string AdvHelpDetailMode => Loc.Get("AdvHelpDetail_Mode");
        public string AdvHelpDetailWorkflow => Loc.Get("AdvHelpDetail_Workflow");
        public string AdvHelpDetailStaging => Loc.Get("AdvHelpDetail_Staging");
        public string AdvHelpDetailPush => Loc.Get("AdvHelpDetail_Push");
        public string AdvHelpDetailBranches => Loc.Get("AdvHelpDetail_Branches");
        public string AdvHelpDetailFiles => Loc.Get("AdvHelpDetail_Files");
        public string AdvHelpDetailReleases => Loc.Get("AdvHelpDetail_Releases");
        public string AdvHelpDetailIdentity => Loc.Get("AdvHelpDetail_Identity");
        public string AdvHelpDetailSafety => Loc.Get("AdvHelpDetail_Safety");
        public string AdvHelpDetailLog => Loc.Get("AdvHelpDetail_Log");
        public string AdvHelpDetailSettings => Loc.Get("AdvHelpDetail_Settings");
        public string AdvHelpDetailAutoAdvanced => Loc.Get("AdvHelpDetail_AutoAdvanced");
        public string AdvHelpDetailAutoProject => Loc.Get("AdvHelpDetail_AutoProject");
        public string AdvHelpDetailAutoStaging => Loc.Get("AdvHelpDetail_AutoStaging");
        public string AdvHelpDetailAutoPush => Loc.Get("AdvHelpDetail_AutoPush");
        public string AdvHelpDetailAutoBranches => Loc.Get("AdvHelpDetail_AutoBranches");
        public string AdvHelpDetailAutoFiles => Loc.Get("AdvHelpDetail_AutoFiles");
        public string AdvHelpDetailAutoReleases => Loc.Get("AdvHelpDetail_AutoReleases");
        public string AdvHelpDetailAutoIdentity => Loc.Get("AdvHelpDetail_AutoIdentity");
        public string AdvHelpDetailAutoSafety => Loc.Get("AdvHelpDetail_AutoSafety");
        public string AdvHelpDetailAutoRun => Loc.Get("AdvHelpDetail_AutoRun");
        public string HintAdvCreateNewRepo => Loc.Get("Hint_AdvCreateNewRepo");
        public string HintDraftBranchName => Loc.Get("Hint_DraftBranchName");
        public string HintRevertHash => Loc.Get("Hint_RevertHash");
        public string HintMergeSource => Loc.Get("Hint_MergeSource");
        public string AutoRunCreateGithubRepo => Loc.Get("Auto_RunCreateGithubRepo");
        public string HintAutoRunCreateGithubRepo => Loc.Get("Hint_AutoRunCreateGithubRepo");
        public string LabelOriginUrl => Loc.Get("Label_OriginUrl");
        public string HintAutoOriginUrl => Loc.Get("Hint_AutoOriginUrl");
        public string BtnApplyOriginUrl => Loc.Get("Btn_ApplyOriginUrl");
        public string HintAdvRepoPath => Loc.Get("Hint_AdvRepoPath");
        public string HintAdvCommitMessage => Loc.Get("Hint_AdvCommitMessage");
        public string HintAdvChangedFiles => Loc.Get("Hint_AdvChangedFiles");
        public string HintAdvFilesSelection => Loc.Get("Hint_AdvFilesSelection");
        public string HintReleaseTag => Loc.Get("Hint_ReleaseTag");
        public string HintReleaseTitle => Loc.Get("Hint_ReleaseTitle");
        public string HintReleaseNotes => Loc.Get("Hint_ReleaseNotes");
        public string HintReleaseTarget => Loc.Get("Hint_ReleaseTarget");
        public string HintReleaseLatest => Loc.Get("Hint_ReleaseLatest");
        public string HintReleasePrerelease => Loc.Get("Hint_ReleasePrerelease");
        public string HintReleaseAssets => Loc.Get("Hint_ReleaseAssets");
        public string HintIdentityName => Loc.Get("Hint_IdentityName");
        public string HintIdentityEmail => Loc.Get("Hint_IdentityEmail");
        public string HintAutoRunPull => Loc.Get("Hint_AutoRunPull");
        public string HintAutoUseSelectedFiles => Loc.Get("Hint_AutoUseSelectedFiles");
        public string HintAutoRunCommit => Loc.Get("Hint_AutoRunCommit");
        public string HintAutoRunPush => Loc.Get("Hint_AutoRunPush");
        public string HintAutoRunCreateRelease => Loc.Get("Hint_AutoRunCreateRelease");
        public string HintEzRepoPath => Loc.Get("Hint_EzRepoPath");
        public string HintEzCommitMessage => Loc.Get("Hint_EzCommitMessage");
        public string TabAutoActions => Loc.Get("Tab_AutoActions");
        public string AutoActionsHeader => Loc.Get("Auto_ActionsHeader");
        public string AutoActionsHint => Loc.Get("Auto_ActionsHint");
        public string AutoRunPull => Loc.Get("Auto_RunPull");
        public string AutoUseSelectedFiles => Loc.Get("Auto_UseSelectedFiles");
        public string AutoRunCommit => Loc.Get("Auto_RunCommit");
        public string AutoRunPush => Loc.Get("Auto_RunPush");
        public string AutoRunCreateRelease => Loc.Get("Auto_RunCreateRelease");
        public string AutoCheckPlan => Loc.Get("Auto_CheckPlan");
        public string AutoRunAll => Loc.Get("Auto_RunAll");
        public string AutoPreviewHeader => Loc.Get("Auto_PreviewHeader");
        public string AutoTxtActionsOnRun => Loc.Get("AutoTxt_ActionsOnRun");

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
