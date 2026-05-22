namespace WpfAutoGitHelper.Models
{
    public sealed class ConflictFileEntry
    {
        public string FilePath { get; set; } = "";
        public string StatusCode { get; set; } = "";
    }
}
