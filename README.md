# WPF Auto Git Helper

Small **Windows WPF** app: run common **Git** actions with buttons for **any** local repository — no terminal required.

## Requirements

- Windows 10/11  
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)  
- [Git for Windows](https://git-scm.com/download/win) (`git.exe` on PATH)  
- [GitHub CLI](https://cli.github.com/) (`gh`) and `gh auth login` — for **Create on GitHub…** and **Release publishing**

## Build

```powershell
cd path\to\WpfAutoGitHelper
$msbuild = "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
# or VS 18: ...\18\Community\MSBuild\Current\Bin\MSBuild.exe
& $msbuild WpfAutoGitHelper.sln /p:Configuration=Release
```

Output: `WpfAutoGitHelper\bin\Release\WpfAutoGitHelper.exe`

Portable data (created on first save): `WpfAutoGitHelper\bin\Release\Data\settings.json` — same folder as the `.exe`.

## First run

1. Launch `WpfAutoGitHelper.exe`.  
2. On the **Actions** tab, set the folder that contains a `.git` directory (clone root) and click **Save**.  
3. On the **Identity** tab, set **user.name** / **user.email** (Load / Apply) — global only.  
4. On first **Push**, Git Credential Manager may prompt you; use a **personal access token** for HTTPS instead of your GitHub password.

## Features

- **Workflow:** Pull, Status, Diff, Add all, Commit, Push; branches (create / checkout / push); **Clear** workflow fields.  
- **Create on GitHub…** — new repo on GitHub via `gh` (name, description, visibility, `.gitignore`, license, README, local clone + push).  
- **Release publishing** — `gh release create` with tag, title, notes, target branch, latest / pre-release.  
- **Explorer** and **GitHub** (when `origin` is GitHub).  
- **15 languages**, **light/dark** theme, auto-saved settings.  
- Auto-generated commit messages are always **English** (`Initial commit`, `Add project files`).  
- Migrates settings from `%AppData%\WpfAutoGitHelper` or legacy `%AppData%\GlocGitHelper\` into `Data\` beside the exe on first launch.

Passwords and PATs are **not** stored in this app.

## SmartScreen

Smartscreen may trigger on startup. If you don't trust the file, compile it from open source.

VirusTotal - 0/71 (try yourself)
Behavior detections - Not found (try yourself)

## Settings

`Data\settings.json` next to `WpfAutoGitHelper.exe` — `Language`, `Theme`, `ConfirmCommit`, `AutoRefreshOnSaveRepo`, recent repo paths, release fields, last commit message.

## License

MIT — see [LICENSE](LICENSE).
