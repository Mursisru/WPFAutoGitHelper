using System.Collections.Generic;

namespace WpfAutoGitHelper.Models
{
    public sealed class ReleaseRequest
    {
        public string Tag { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }
        public string TargetBranch { get; set; }
        public bool IsLatest { get; set; }
        public bool IsPrerelease { get; set; }
        public IList<string> AssetPaths { get; set; }
    }
}
