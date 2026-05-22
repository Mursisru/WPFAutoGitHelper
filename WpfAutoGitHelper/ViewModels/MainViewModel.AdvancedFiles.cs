using System.IO;
using System.Threading.Tasks;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Models;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private async Task GitRmSelectedFileAsync()
        {
            var file = SelectedChangedFile;
            if (file == null)
                return;

            if (!await _dangerGuard.ConfirmAsync(DangerousOperationType.FileDelete, Loc.Get("Btn_GitRm"), file.FilePath, null).ConfigureAwait(true))
                return;

            await RunGitLoggedAsync("rm", "--", file.FilePath).ConfigureAwait(true);
            await RefreshStatusAsync().ConfigureAwait(true);
        }

        private async Task DeleteUntrackedFileAsync()
        {
            var file = SelectedChangedFile;
            if (file == null || !file.IsUntracked)
                return;

            var full = Path.Combine(RepoPath, file.FilePath.Replace('/', Path.DirectorySeparatorChar));
            if (!await _dangerGuard.ConfirmAsync(DangerousOperationType.FileDelete, Loc.Get("Btn_DeleteUntracked"), file.FilePath, null).ConfigureAwait(true))
                return;

            if (File.Exists(full))
                File.Delete(full);
            else if (Directory.Exists(full))
                Directory.Delete(full, true);

            await RefreshStatusAsync().ConfigureAwait(true);
        }
    }
}
