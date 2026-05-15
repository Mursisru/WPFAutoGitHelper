using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WpfAutoGitHelper.Services
{
    internal static class GitignoreTemplates
    {
        private static readonly Dictionary<string, string> RemotePaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["visualstudio"] = "VisualStudio.gitignore",
            ["dotnet"] = "VisualStudio.gitignore",
            ["node"] = "Node.gitignore",
            ["python"] = "Python.gitignore",
            ["unity"] = "Unity.gitignore",
        };

        private static readonly Dictionary<string, string> Fallbacks = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["visualstudio"] = @"## Visual Studio / .NET
bin/
obj/
.vs/
*.user
*.suo
*.cache
*.dll
*.pdb
packages/
TestResults/
",
            ["dotnet"] = @"## .NET
bin/
obj/
.vs/
*.user
*.suo
project.lock.json
project.fragment.lock.json
artifacts/
",
            ["node"] = @"## Node
node_modules/
npm-debug.log*
dist/
.env
.DS_Store
",
            ["python"] = @"## Python
__pycache__/
*.py[cod]
.venv/
venv/
.env
*.egg-info/
.pytest_cache/
",
            ["unity"] = @"## Unity
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
UserSettings/
*.csproj
*.sln
*.user
*.unityproj
*.pidb
*.booproj
",
        };

        public static async Task<string> GetContentAsync(string gitignoreId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(gitignoreId) ||
                string.Equals(gitignoreId, "none", StringComparison.OrdinalIgnoreCase))
                return null;

            if (!RemotePaths.TryGetValue(gitignoreId, out var remoteFile))
                return null;

            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(12) })
                {
                    var url = "https://raw.githubusercontent.com/github/gitignore/main/" + remoteFile;
                    var content = await client.GetStringAsync(url).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(content))
                        return content;
                }
            }
            catch
            {
                // use fallback
            }

            cancellationToken.ThrowIfCancellationRequested();
            return Fallbacks.TryGetValue(gitignoreId, out var fallback) ? fallback : null;
        }
    }
}
