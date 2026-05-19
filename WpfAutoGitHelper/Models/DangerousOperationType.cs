namespace WpfAutoGitHelper.Models
{
    public enum DangerousOperationType
    {
        ForcePush,
        ForcePushLease,
        DeleteRemoteBranch,
        DeleteLocalBranch,
        HardReset,
        Rebase,
        Merge,
        Revert,
        Amend,
        FileDelete,
    }
}
