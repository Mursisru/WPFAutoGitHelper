# Publishing to GitHub

**Canonical push folder:** `C:\Users\at747\OneDrive\Desktop\GITHUB local\WpfAutoGitHelper\`

**Development copy:** `C:\Users\at747\source\repos\WpfAutoGitHelper\`

After changes in `source\repos`, sync to GITHUB local (exclude build output):

```powershell
robocopy "C:\Users\at747\source\repos\WpfAutoGitHelper" "C:\Users\at747\OneDrive\Desktop\GITHUB local\WpfAutoGitHelper" /E /XD bin obj .vs .git /XF *.user /NFL /NDL /NJH /NJS /nc /ns /np
```

## First push

1. Create an empty repository on GitHub (e.g. `WpfAutoGitHelper`), without README if you already have one locally.  
2. From the GITHUB local folder:

```powershell
cd "C:\Users\at747\OneDrive\Desktop\GITHUB local\WpfAutoGitHelper"
git init
git add .
git commit -m "Initial release: WPF Auto Git Helper v1.1.0"
git branch -M main
git remote add origin https://github.com/<USER>/WpfAutoGitHelper.git
git push -u origin main
```

## About (GitHub repository description)

- **EN:** `Windows WPF Git GUI — pull, commit, push, branches, new repo wizard with GitHub CLI, 15 languages, light/dark theme.`
- **RU:** `WPF-приложение для Git без терминала: pull, commit, push, ветки, мастер нового репозитория и GitHub CLI, 15 языков, светлая/тёмная тема.`

## Releases

Attach `WpfAutoGitHelper\bin\Release\WpfAutoGitHelper.exe` (build Release first) or zip the Release folder.
