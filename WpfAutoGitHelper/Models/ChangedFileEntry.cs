namespace WpfAutoGitHelper.Models
{
    public sealed class ChangedFileEntry
    {
        public string StatusCode { get; set; } = "";
        public string FilePath { get; set; } = "";
        public bool IsUntracked => StatusCode == "??";
    }
}
