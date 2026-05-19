using System;
using System.Collections.ObjectModel;
using System.Linq;
using WpfAutoGitHelper.Localization;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        public ObservableCollection<string> BranchesForPicker { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> RemoteBranchesForPicker { get; } = new ObservableCollection<string>();

        private string BranchPickerNoneLabel => Loc.Get("Combo_None");

        private bool IsBranchPickerNone(string value) =>
            string.IsNullOrWhiteSpace(value) ||
            string.Equals(value, BranchPickerNoneLabel, StringComparison.Ordinal);

        private void RebuildBranchPickerLists()
        {
            var none = BranchPickerNoneLabel;
            var localSel = SelectedLocalBranchForDelete;
            var remoteSel = SelectedRemoteBranchForDelete;
            var mergeSel = MergeSourceBranch;
            var localWasNone = IsBranchPickerNone(localSel);
            var remoteWasNone = IsBranchPickerNone(remoteSel);
            var mergeWasNone = IsBranchPickerNone(mergeSel);

            BranchesForPicker.Clear();
            BranchesForPicker.Add(none);
            foreach (var b in Branches)
            {
                if (!string.IsNullOrWhiteSpace(b))
                    BranchesForPicker.Add(b);
            }

            RemoteBranchesForPicker.Clear();
            RemoteBranchesForPicker.Add(none);
            foreach (var b in RemoteBranches)
            {
                if (!string.IsNullOrWhiteSpace(b))
                    RemoteBranchesForPicker.Add(b);
            }

            SelectedLocalBranchForDelete = localWasNone || !Branches.Contains(localSel)
                ? none
                : localSel;
            SelectedRemoteBranchForDelete = remoteWasNone || !RemoteBranches.Contains(remoteSel)
                ? none
                : remoteSel;

            if (mergeWasNone || string.IsNullOrWhiteSpace(mergeSel))
                MergeSourceBranch = none;
            else if (Branches.Contains(mergeSel))
                MergeSourceBranch = mergeSel;
            else if (!IsBranchPickerNone(mergeSel))
                MergeSourceBranch = mergeSel;
        }
    }
}
