# Pre-release — WPF Auto Git Helper

**Version:** `1.5.0 Build PR-R1P8`  
**Date:** 2026-05-19  
**Channel:** `GITHUB local\pre-release\WpfAutoGitHelper`  
**Target GitHub release:** 1.5.0 (public site still 1.4.4 until release)

## Build

- **Exe:** `WpfAutoGitHelper\bin\Release\WpfAutoGitHelper.exe`
- **Framework:** .NET Framework 4.8 (WPF)

## This build (PR-R1P8)

- **Auto — autonomous rebase recovery:** stage + amend, resolve conflicts (keep working tree / Ours for CHANGELOG, PRE-RELEASE, VERSION, csproj), `checkout --ours` fallback, `rebase --skip` for stuck duplicate picks, then `rebase --continue` in a loop.
- **Auto — branch during rebase:** detects `development-test` from rebase metadata when `branch --show-current` is empty.
- **Auto — pull:** skipped while interactive rebase is in progress.

## Notes

- Draft push: `git push origin HEAD:refs/heads/<name>` (branch on GitHub, no local checkout).
- Empty draft field on Run = normal push of current branch.
