namespace WpfAutoGitHelper.Models
{
    public sealed class GitRunResult
    {
        public int ExitCode { get; set; }
        public string StandardOutput { get; set; } = "";
        public string StandardError { get; set; } = "";
        public bool Success => ExitCode == 0;
    }
}
