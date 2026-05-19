# Pre-release — WPF Auto Git Helper

**Version:** `1.5.0 Build PR-R1P7`  
**Date:** 2026-05-19  
**Channel:** `GITHUB local\pre-release\WpfAutoGitHelper`  
**Target GitHub release:** 1.5.0 (public site still 1.4.4 until release)

## Build

- **Exe:** `WpfAutoGitHelper\bin\Release\WpfAutoGitHelper.exe`
- **Framework:** .NET Framework 4.8 (WPF)

## This build (PR-R1P7)

- **Draft branch on GitHub:** `git push origin HEAD:refs/heads/<name>` — remote branch is created/updated without local `checkout -b`.
- **Auto Advanced:** validation no longer blocks when files exist but checkboxes are empty (`git add -A` fallback); files auto-selected on refresh in Auto mode.
- **Auto Advanced:** pull skipped (with log) when working tree is dirty or sync with origin fails — scenario continues.

## Notes

- Empty draft field on **Run** = normal push of current branch; filled field = push to named branch on **origin**.
- Requires **origin** URL on Project tab (Auto) before push.
