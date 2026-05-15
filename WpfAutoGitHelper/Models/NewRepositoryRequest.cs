using System.IO;

namespace WpfAutoGitHelper.Models
{
    public sealed class NewRepositoryRequest
    {
        public string ParentDirectory { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string GitignoreId { get; set; }
        public string LicenseId { get; set; }
        public bool AddReadme { get; set; }
        public bool IsPrivate { get; set; }

        public string FullPath =>
            string.IsNullOrWhiteSpace(ParentDirectory) || string.IsNullOrWhiteSpace(Name)
                ? ""
                : Path.Combine(ParentDirectory.Trim(), Name.Trim());
    }
}
