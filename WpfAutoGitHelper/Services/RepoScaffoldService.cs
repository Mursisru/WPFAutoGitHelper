using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WpfAutoGitHelper.Models;

namespace WpfAutoGitHelper.Services
{
    public static class RepoScaffoldService
    {
        public static string SanitizeRepoName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            var s = name.Trim();
            foreach (var c in Path.GetInvalidFileNameChars())
                s = s.Replace(c.ToString(), "");

            s = s.Replace(' ', '-');
            while (s.Contains("--"))
                s = s.Replace("--", "-");

            return s.Trim('.', '-');
        }

        public static async Task ApplyAsync(NewRepositoryRequest request, string copyrightHolder, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var path = request.FullPath;
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("Repository path is empty.");

            Directory.CreateDirectory(path);

            var gitignore = await GitignoreTemplates.GetContentAsync(request.GitignoreId, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(gitignore))
                File.WriteAllText(Path.Combine(path, ".gitignore"), gitignore);

            if (LicenseTexts.TryGet(request.LicenseId, copyrightHolder, out var license))
                File.WriteAllText(Path.Combine(path, "LICENSE"), license);

            var wroteAny = !string.IsNullOrWhiteSpace(gitignore)
                || LicenseTexts.TryGet(request.LicenseId, copyrightHolder, out _);

            if (request.AddReadme)
            {
                var readme = BuildReadme(request.Name, request.Description);
                File.WriteAllText(Path.Combine(path, "README.md"), readme);
                wroteAny = true;
            }

            if (!wroteAny)
                File.WriteAllText(Path.Combine(path, ".gitkeep"), "");
        }

        private static string BuildReadme(string name, string description)
        {
            var title = string.IsNullOrWhiteSpace(name) ? "Project" : name.Trim();
            if (string.IsNullOrWhiteSpace(description))
                return "# " + title + "\n";

            return "# " + title + "\n\n" + description.Trim() + "\n";
        }
    }
}
