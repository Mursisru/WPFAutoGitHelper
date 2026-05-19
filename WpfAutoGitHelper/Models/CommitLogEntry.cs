namespace WpfAutoGitHelper.Models
{
    public sealed class CommitLogEntry
    {
        public string Hash { get; set; } = "";
        public string ShortHash { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Display => string.IsNullOrEmpty(Hash) ? Subject : ShortHash + "  " + Subject;
    }
}
