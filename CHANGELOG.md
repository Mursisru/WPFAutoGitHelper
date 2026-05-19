# Changelog

## 1.5.0 Build PR-R1P9 — 2026-05-19 (pre-release)

### Fixed
- **Auto Run:** when working tree is clean (e.g. after successful rebase), staging and commit are skipped instead of failing with «nothing to commit» — push/draft steps still run.

## 1.5.0 Build PR-R1P8 — 2026-05-19 (pre-release)

### Added
- **Auto Advanced — autonomous git recovery:** interactive rebase handled automatically (stage, amend, conflict resolution, continue/skip loop).
- **Auto conflict resolve:** keeps working-tree files; resolves markers in CHANGELOG / PRE-RELEASE / VERSION / csproj (Ours); fallback `checkout --ours`; skip duplicate rebase picks when needed.

### Fixed
- **Current branch** detectable during interactive rebase (`development-test (rebase)` in UI).
- **Pull** not run while rebase is in progress.

## 1.5.0 Build PR-R1P7 — 2026-05-19 (pre-release)

### Changed
- **Draft branch push** creates/updates the branch on **GitHub** (`origin`) via `HEAD:refs/heads/<name>` — no local `checkout -b`.
- **Auto Run:** empty draft field → normal push; filled field → draft push on origin.

### Fixed
- **Auto validation:** no false «no files selected» when changes exist but checkboxes are empty (`git add -A` fallback; auto-select on refresh in Auto).
- **Auto pull:** skipped with log when working tree is dirty or pull fails (unrelated histories, etc.) — run continues.

## 1.5.0 Build PR-R1P6 — 2026-05-19 (pre-release)

### Fixed
- Busy state lock (`Busy: True`) after long/failed Auto runs.
- Removed recursive refresh loop in Auto preview/status flow.
- Unified busy scope for long workflows to avoid nested busy toggles/cancellation races.
- Improved git process cancellation/exit handling to prevent stuck operations.

## 1.5.0 Build PR-R1P5 — 2026-05-19 (pre-release)

### Fixed
- **Auto Run** on new repo (no commits): auto `git add -A` for initial commit when nothing is checked.
- **Auto Run** stops on first error (no false «finished» / «draft sent» after failed steps).
- **Commit / draft push** report failure when `git` exits non-zero.
- **Pull** skipped when there are no commits; uses **origin/main** when local branch has no remote ref.

## 1.5.0 Build PR-R1P4 — 2026-05-19 (pre-release)

Pre-release channel copy (`GITHUB local\pre-release\WpfAutoGitHelper`). Target GitHub release: **1.5.0**.

### Added / changed (since 1.4.4)
- **EZ / Advanced / Auto Advanced** UI modes (F7, MSI BIOS segment toggle).
- **Auto Advanced:** all tabs visible; unique tab names; Run scenario; origin URL on Project tab; file checkboxes; Create GitHub repo on Run.
- **Advanced:** index, push & draft, branches & merge, file manager, safety log, nested help tabs, field hints.
- **Branch pickers:** **(none)** option for delete local/remote, merge source, revert commit list.
- **Push & draft:** empty draft field = normal push; filled = draft branch push.
- **Spellcheck** only on commit message, release title/notes, new repo description.

## 1.4.4 — 2026-05-16

### Changed
- **Origin URL** is requested on every **Push**, **Sync to GitHub**, **Push branch**, and **Create release** (in-app dialog, not only when remote is missing).
- **All messages** (errors, confirmations, URL input) are shown **inside the app** — no system MessageBox or VB InputBox.

### Added
- In-app dialog overlay (OK, Yes/No, text field for remote URL).
- Startup/fatal error window in the application theme.

## 1.4.3 — 2026-05-16

### Fixed
- **Card headers** (all tabs): removed top gap/misaligned header strip — header is flush with the card border (`ClipToBounds`, no inner top margin).

### Added
- **7 more accent colors:** red, cyan, lime, indigo, pink, gold, sky (14 total).
- **6 more backgrounds:** stone, ocean, plum, dusk, slate, cherry (13 total).

## 1.4.2 — 2026-05-16

### Fixed
- **Freeze when changing theme or accent** — removed infinite loop (`AccentChanged` / `ThemeChanged` handlers re-applied appearance recursively).

### Added
- **Background** color picker in Settings (default, cool, warm, mint, navy, graphite, espresso) — tints panels and cards per theme mode.

## 1.4.1 — 2026-05-16

### Added
- **Accent color** picker in Settings (blue, teal, green, orange, purple, rose, amber) — applies to tabs, buttons, branch badge, and scrollbar thumb when dragging.

### Changed
- Active tab underline spans the **full tab width**; tab borders are **thicker** (2px).
- **Current branch** status panel has a visible accent border (2px).
- **Scrollbars** styled to match the theme (slim track, rounded thumb, accent on drag).

## 1.4.0 — 2026-05-16

### Changed
- **Tab order:** Release publishing now comes before Git identity.
- **UI redesign** for Light, Dark, and Black themes: shared chrome, accent cards, pill branch badge, refined tabs with accent underline, improved inputs and buttons.

## 1.3.1 — 2026-05-16

### Fixed
- **Sync to GitHub** pulls remote changes **before** committing, avoiding modify/delete conflicts when GitHub removed a file (e.g. `WPFAutoGitHelper_v1.2.1.zip`) but the local tree still had it.
- Pull uses **`--autostash`**; on rebase conflicts with obsolete root release `.zip` files, the app can remove them and continue (or abort rebase and fall back to merge pull).
- Recovers from an **interrupted rebase** left by a failed previous sync.

### Changed
- Root release zips (`WPFAutoGitHelper_v*.zip`) are listed in **`.gitignore`** (use GitHub Releases for binaries).

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
