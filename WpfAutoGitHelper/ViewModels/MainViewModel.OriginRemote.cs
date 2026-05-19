using System;
using System.Threading.Tasks;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Models;
using WpfAutoGitHelper.Services;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private string _originRemoteUrl = "";

        public string OriginRemoteUrl
        {
            get => _originRemoteUrl;
            set
            {
                var v = value ?? "";
                if (_originRemoteUrl == v)
                    return;
                _originRemoteUrl = v;
                OnPropertyChanged();
                ApplyOriginRemoteUrlCommand?.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand ApplyOriginRemoteUrlCommand { get; private set; }

        private void InitOriginRemote()
        {
            OriginRemoteUrl = _settings.LastOriginRemoteUrl ?? "";
            ApplyOriginRemoteUrlCommand = new RelayCommand(
                async () => await ApplyOriginRemoteUrlAsync(log: true),
                () => HasValidRepo && !IsBusy && !string.IsNullOrWhiteSpace(OriginRemoteUrl));
        }

        private async Task RefreshOriginRemoteUrlAsync()
        {
            if (!HasValidRepo)
                return;

            var remote = await RunGitQuietAsync("remote", "get-url", "origin");
            if (remote.Success && !string.IsNullOrWhiteSpace(remote.StandardOutput))
            {
                OriginRemoteUrl = remote.StandardOutput.Trim();
                return;
            }

            if (string.IsNullOrWhiteSpace(OriginRemoteUrl) && !string.IsNullOrWhiteSpace(_settings.LastOriginRemoteUrl))
                OriginRemoteUrl = _settings.LastOriginRemoteUrl;
            else if (string.IsNullOrWhiteSpace(OriginRemoteUrl) && !string.IsNullOrWhiteSpace(_settings.CachedGitHubUrl))
                OriginRemoteUrl = GitRunner.ToGitRemoteUrl(_settings.CachedGitHubUrl);
        }

        private async Task<bool> ApplyOriginRemoteUrlAsync(bool log = true)
        {
            if (!HasValidRepo)
                return false;

            var url = OriginRemoteUrl?.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                var existing = await RunGitQuietAsync("remote", "get-url", "origin");
                return existing.Success && !string.IsNullOrWhiteSpace(existing.StandardOutput);
            }

            url = GitRunner.ToGitRemoteUrl(url);
            var current = await RunGitQuietAsync("remote", "get-url", "origin");
            var currentUrl = current.Success ? current.StandardOutput?.Trim() : "";

            GitRunResult result;
            if (string.IsNullOrEmpty(currentUrl))
            {
                if (log)
                    AppendLog(Loc.Get("Msg_NoOrigin"), true);
                result = await RunGitLoggedAsync("remote", "add", "origin", url);
                if (result.Success && log)
                    AppendLog(string.Format(Loc.Get("Msg_RemoteAdded"), url));
            }
            else if (string.Equals(currentUrl, url, StringComparison.OrdinalIgnoreCase))
            {
                result = new GitRunResult { ExitCode = 0 };
            }
            else
            {
                result = await RunGitLoggedAsync("remote", "set-url", "origin", url);
                if (result.Success && log)
                    AppendLog(string.Format(Loc.Get("Msg_RemoteUpdated"), url));
            }

            if (!result.Success)
                return false;

            OriginRemoteUrl = url;
            _settings.LastOriginRemoteUrl = url;
            var web = GitRunner.ToGitHubWebUrl(url);
            if (!string.IsNullOrWhiteSpace(web))
                _settings.CachedGitHubUrl = web;
            SettingsStore.Save(_settings);
            return true;
        }

        private async Task<bool> EnsureOriginForAutoAsync()
        {
            if (!string.IsNullOrWhiteSpace(OriginRemoteUrl))
                return await ApplyOriginRemoteUrlAsync(log: true).ConfigureAwait(true);

            var remote = await RunGitQuietAsync("remote", "get-url", "origin");
            if (remote.Success && !string.IsNullOrWhiteSpace(remote.StandardOutput))
            {
                OriginRemoteUrl = remote.StandardOutput.Trim();
                return true;
            }

            await NotifyAsync(Loc.Get("Msg_OriginUrlRequired"), isError: true).ConfigureAwait(true);
            return false;
        }
    }
}
