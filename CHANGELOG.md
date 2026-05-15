# Changelog

## 1.1.0 — 2026-05-15

### Added
- **New repository** wizard: name, description, `.gitignore` template, license, README, public/private visibility.
- Optional **GitHub publish** via GitHub CLI (`gh repo create` + push).
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
