**Developer:** Mursisru

# WPF Auto Git Helper

[![.NET Framework 4.8](https://img.shields.io/badge/Platform-.NET%20Framework%204.8-512BD4)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![Version](https://img.shields.io/badge/Version-1.5.0-green)](https://github.com/Mursisru/WPFAutoGitHelper/releases/tag/v1.5.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow)](https://github.com/Mursisru/WPFAutoGitHelper/blob/main/LICENSE)

---

## Critical warnings

> [!IMPORTANT]
> **Git for Windows on PATH** - `git.exe` required for all workflows.

> [!IMPORTANT]
> **GitHub CLI (`gh`) required** for **Create on GitHub** and **Release publishing** - run `gh auth login` first.

> [!WARNING]
> **Advanced mode includes destructive Git actions** - force-with-lease, amend, and branch delete require explicit confirmation; review the safety log.

> [!NOTE]
> **Passwords and PATs are not stored** - use Git Credential Manager; HTTPS needs a personal access token, not your GitHub password.

Small **Windows WPF** app: run common **Git** actions with buttons for **any** local repository — no terminal required.

**Current version: 1.5.0**

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
2. Choose **EZ**, **Advanced**, or **Auto Advanced** in the header (or press **F7** to cycle).  
3. Set the repository folder (clone root) and click **Save**.  
4. On the **Identity** tab, set **user.name** / **user.email** (Load / Apply) — global only.  
5. On first **Push**, Git Credential Manager may prompt you; use a **personal access token** for HTTPS instead of your GitHub password.

## Features

### EZ mode
- Workflow: Pull, Status, Diff, Add all, Commit, Push; branches; **Clear** workflow fields.  
- **Create on GitHub…** — new repo via `gh` (visibility, `.gitignore`, license, README, clone + push).  
- **Release publishing** — `gh release create` (tag, title, notes, assets, latest / pre-release).  
- **Explorer** and **GitHub** when `origin` points to GitHub.

### Advanced mode
- Selective stage/commit/push, amend, revert, draft branch push, force-with-lease.  
- Merge/rebase, conflict list + resolution UI, local/remote branch delete.  
- Changed-files list with checkboxes; safety log for confirmed dangerous ops.  
- In-app help tabs per section.

### Auto Advanced mode
- Same tabs visible with Auto-oriented labels; **Run** tab for one-click scenario.  
- Options: pull, selected files only, commit, push, optional GitHub repo/release.  
- Origin URL on Project tab; preview before run.

### UI and settings
- **15 languages**; **Light / Dark / Black** themes; **accent** and **background** presets.  
- **Default UI mode** and **field hints** in Settings.  
- **Context menus** on repo path, file lists, text fields, log, release assets.  
- In-app dialogs (no system MessageBox for normal flows).  
- Auto-generated commit messages are always **English**.  
- Settings in `Data\settings.json` next to the exe (migrates from legacy AppData paths).

Passwords and PATs are **not** stored in this app.

## Settings

`Data\settings.json` next to `WpfAutoGitHelper.exe` — language, theme, accent, background, UI mode, confirm commit, field hints, recent repos, release fields, origin URL cache, and more.

## License

MIT — see [LICENSE](LICENSE).

---

## Keywords

windows, wpf, dotnet-framework, wpfautogithelper, csharp
