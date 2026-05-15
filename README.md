# WPF Auto Git Helper

Small **Windows WPF** app: run common **Git** actions with buttons for **any** local repository — no terminal required.

## Requirements

- Windows 10/11  
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)  
- [Git for Windows](https://git-scm.com/download/win) (`git.exe` on PATH)  
- Optional: [GitHub CLI](https://cli.github.com/) (`gh`) for **Create on GitHub** when making a new repository (`gh auth login`)

## Build

```powershell
cd path\to\WpfAutoGitHelper
$msbuild = "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
# or VS 18: ...\18\Community\MSBuild\Current\Bin\MSBuild.exe
& $msbuild WpfAutoGitHelper.sln /p:Configuration=Release
```

Output: `WpfAutoGitHelper\bin\Release\WpfAutoGitHelper.exe`

## First run

1. Launch `WpfAutoGitHelper.exe`.  
2. On the **Actions** tab, set the folder that contains a `.git` directory (clone root) and click **Save**.  
3. On the **Identity** tab, set **user.name** / **user.email** (Load / Apply) — global only.  
4. On first **Push**, Git Credential Manager may prompt you; use a **personal access token** for HTTPS instead of your GitHub password.

## Features

- **Workflow:** Pull, Status, Diff, Add all, Commit, Push; branches (create / checkout / push).  
- **Files** tab: changed files list, discard (`git restore`).  
- **New repository…** wizard: name, description, `.gitignore`, license, README, public/private; optional `gh repo create` + push.  
- **Explorer** and **GitHub** (when `origin` is GitHub).  
- **15 languages**, **light/dark** theme, auto-saved settings.  
- Recent repo paths in `%AppData%\WpfAutoGitHelper\settings.json`.  
- Migrates settings from legacy `%AppData%\GlocGitHelper\settings.json` on first launch.

Passwords and PATs are **not** stored in this app.

## Settings

`%AppData%\WpfAutoGitHelper\settings.json` — `Language`, `Theme`, `ConfirmCommit`, `ConfirmRestore`, `AutoRefreshOnSaveRepo`, recent paths.

## License

MIT — see [LICENSE](LICENSE).
