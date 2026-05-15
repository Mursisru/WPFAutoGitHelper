using System.Collections.Generic;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Models;

namespace WpfAutoGitHelper.Services
{
    public static class RepoScaffoldCatalog
    {
        public static IReadOnlyList<TemplateOption> GitignoreOptions { get; } = new[]
        {
            new TemplateOption("none", Loc.Get("NewRepo_Gitignore_None")),
            new TemplateOption("visualstudio", Loc.Get("NewRepo_Gitignore_VisualStudio")),
            new TemplateOption("dotnet", Loc.Get("NewRepo_Gitignore_Dotnet")),
            new TemplateOption("node", Loc.Get("NewRepo_Gitignore_Node")),
            new TemplateOption("python", Loc.Get("NewRepo_Gitignore_Python")),
            new TemplateOption("unity", Loc.Get("NewRepo_Gitignore_Unity")),
        };

        public static IReadOnlyList<TemplateOption> LicenseOptions { get; } = new[]
        {
            new TemplateOption("none", Loc.Get("NewRepo_License_None")),
            new TemplateOption("mit", "MIT"),
            new TemplateOption("apache-2.0", "Apache License 2.0"),
            new TemplateOption("gpl-3.0", "GNU GPLv3"),
            new TemplateOption("bsd-2-clause", "BSD 2-Clause"),
            new TemplateOption("unlicense", "The Unlicense"),
        };
    }
}
