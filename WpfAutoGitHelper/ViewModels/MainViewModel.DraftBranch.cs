using System.Threading.Tasks;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private async Task<bool> HasWorkingTreeChangesAsync()
        {
            var status = await RunGitQuietAsync("status", "--porcelain").ConfigureAwait(true);
            return status.Success && !string.IsNullOrWhiteSpace(status.StandardOutput);
        }
    }
}
