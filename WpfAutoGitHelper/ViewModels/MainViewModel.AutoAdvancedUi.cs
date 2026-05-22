using WpfAutoGitHelper.Localization;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        public string AdvTabWorkflow => Loc.Get(IsAutoAdvancedMode ? "AutoTab_Project" : "Tab_Workflow");
        public string AdvTabStaging => Loc.Get(IsAutoAdvancedMode ? "AutoTab_FilesSelect" : "Tab_Staging");
        public string AdvTabPushAdv => Loc.Get(IsAutoAdvancedMode ? "AutoTab_PushOptions" : "Tab_PushAdv");
        public string AdvTabBranchesAdv => Loc.Get(IsAutoAdvancedMode ? "AutoTab_Branches" : "Tab_BranchesAdv");
        public string AdvTabFiles => Loc.Get(IsAutoAdvancedMode ? "AutoTab_FileReview" : "Tab_Files");
        public string AdvTabReleases => Loc.Get(IsAutoAdvancedMode ? "AutoTab_Release" : "Tab_Releases");
        public string AdvTabIdentity => Loc.Get(IsAutoAdvancedMode ? "AutoTab_Identity" : "Tab_Identity");
        public string AdvTabSafety => Loc.Get(IsAutoAdvancedMode ? "AutoTab_Safety" : "Tab_Safety");
        public string AdvTabLog => Loc.Get("Tab_Log");
        public string AdvTabSettings => Loc.Get("Tab_Settings");
        public string AdvTabAutoActions => Loc.Get(IsAutoAdvancedMode ? "AutoTab_Run" : "Tab_AutoActions");
        public string AdvTabHelp => Loc.Get(IsAutoAdvancedMode ? "AutoTab_Help" : "Tab_AdvancedHelp");

        public string AdvHdrStep1Repo => Loc.Get(IsAutoAdvancedMode ? "AutoHdr_ProjectFolder" : "Step1_Repo");
        public string AdvHdrStep2Actions => Loc.Get(IsAutoAdvancedMode ? "AutoHdr_ActionsOnRun" : "Step2_Actions");
        public string AdvHdrFilesChanged => Loc.Get(IsAutoAdvancedMode ? "AutoHdr_FilesForCommit" : "Files_ChangedHeader");
        public string AdvHdrStep4Publish => Loc.Get(IsAutoAdvancedMode ? "AutoHdr_CommitMessage" : "Step4_Publish");
        public string AutoModeBanner => Loc.Get("Auto_ModeBanner");
        public string AutoTxtActionsOnRun => Loc.Get("AutoTxt_ActionsOnRun");

        public string AutoIntroWorkflow => Loc.Get("AutoIntro_Project");
        public string AutoIntroStaging => Loc.Get("AutoIntro_Staging");
        public string AutoIntroPush => Loc.Get("AutoIntro_Push");
        public string AutoIntroBranches => Loc.Get("AutoIntro_Branches");
        public string AutoIntroFiles => Loc.Get("AutoIntro_Files");
        public string AutoIntroReleases => Loc.Get("AutoIntro_Releases");
        public string AutoIntroIdentity => Loc.Get("AutoIntro_Identity");
        public string AutoIntroSafety => Loc.Get("AutoIntro_Safety");
        public string AutoIntroRun => Loc.Get("AutoIntro_Run");

        private void NotifyAdvUiLabels()
        {
            OnPropertyChanged(nameof(AdvTabWorkflow));
            OnPropertyChanged(nameof(AdvTabStaging));
            OnPropertyChanged(nameof(AdvTabPushAdv));
            OnPropertyChanged(nameof(AdvTabBranchesAdv));
            OnPropertyChanged(nameof(AdvTabFiles));
            OnPropertyChanged(nameof(AdvTabReleases));
            OnPropertyChanged(nameof(AdvTabIdentity));
            OnPropertyChanged(nameof(AdvTabSafety));
            OnPropertyChanged(nameof(AdvTabLog));
            OnPropertyChanged(nameof(AdvTabSettings));
            OnPropertyChanged(nameof(AdvTabAutoActions));
            OnPropertyChanged(nameof(AdvTabHelp));
            OnPropertyChanged(nameof(AdvHdrStep1Repo));
            OnPropertyChanged(nameof(AdvHdrStep2Actions));
            OnPropertyChanged(nameof(AdvHdrFilesChanged));
            OnPropertyChanged(nameof(AdvHdrStep4Publish));
            OnPropertyChanged(nameof(AutoModeBanner));
            OnPropertyChanged(nameof(AutoTxtActionsOnRun));
            OnPropertyChanged(nameof(AutoIntroWorkflow));
            OnPropertyChanged(nameof(AutoIntroStaging));
            OnPropertyChanged(nameof(AutoIntroPush));
            OnPropertyChanged(nameof(AutoIntroBranches));
            OnPropertyChanged(nameof(AutoIntroFiles));
            OnPropertyChanged(nameof(AutoIntroReleases));
            OnPropertyChanged(nameof(AutoIntroIdentity));
            OnPropertyChanged(nameof(AutoIntroSafety));
            OnPropertyChanged(nameof(AutoIntroRun));
        }
    }
}
