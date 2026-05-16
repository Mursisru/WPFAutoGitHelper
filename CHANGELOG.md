# Changelog

## 1.3.0 — 2026-05-16

### Added
- **Sync to GitHub** — one-click workflow: stage all changes, commit, pull remote updates (rebase with merge fallback), then push.
- **Origin URL…** button to view or change the `origin` remote without blocking every push.

### Changed
- **Push** uses the same automated pipeline as Sync (auto-commit, auto-pull on rejection, retry push) instead of showing “do Pull / Add all” dialogs.
- **Pull** runs `git fetch` first and sets upstream tracking when missing.
- **origin** URL dialog appears only when `origin` is not configured (or when you click Origin URL…).

### Fixed
- Push no longer stops on uncommitted changes or non-fast-forward errors when a pull can integrate remote commits.

## 1.2.1 — 2026-05-15

### Changed
- **Create release** prompts for the **origin** remote URL (same dialog as Push) before `gh release create`, so you do not need to Push first only to configure the remote.

## 1.2.0 — 2026-05-15

### Added
- **Black** UI theme (true black background) in addition to Light and Dark.
- **Release publishing** tab: `gh release create` (tag, title, notes, target branch, latest / pre-release).
- **Clear** button on Actions tab (reset repo path, branches, commit message).
- `Loc.GetEnglish()` for auto-generated Git commit messages (always English).

### Changed
- **Push** always shows a dialog to confirm or change the **origin** URL before uploading.
- Settings and app data stored in **`Data\` next to the executable** (`Data\settings.json`), not `%AppData%`.
- First launch migrates settings from `%AppData%\WpfAutoGitHelper` or legacy `GlocGitHelper`.
- Removed **Files** tab (full-repo diff on Actions tab).
- Latest / pre-release options are mutually exclusive.

### Fixed
- Post-copy push after **Create on GitHub…** uses the repository’s current branch instead of hardcoded `main`.

## 1.1.0 — 2026-05-15

### Added
- **Create on GitHub…** wizard: creates repo on GitHub via `gh repo create`, local clone, push; name, description, visibility, `.gitignore`, license, README.
- **15 UI languages** with live theme/language switching.
- **Light / Dark** themes with custom tab and input styles.
- Workflow tabs: Actions, Identity (global git config), Files, Log, Settings.

### Fixed
- Language switch refreshes tab headers and bound strings.
- Radio buttons and help panels readable in both themes.
- Help panel background follows selected theme.

## 1.0.0 — initial

- Pull, Status, Diff, Add all, Commit, Push for any local repo.
- Branch create/checkout/push, file restore, Explorer + GitHub link.
- Settings in `%AppData%\WpfAutoGitHelper\settings.json`.
- Migration from legacy `GlocGitHelper` AppData.
