# Pre-release — WPF Auto Git Helper

**Version:** `1.5.0 Build PR-R1P9`  
**Date:** 2026-05-19  
**Channel:** `GITHUB local\pre-release\WpfAutoGitHelper`  
**Target GitHub release:** 1.5.0 (public site still 1.4.4 until release)

## Build

- **Exe:** `WpfAutoGitHelper\bin\Release\WpfAutoGitHelper.exe`
- **Framework:** .NET Framework 4.8 (WPF)

## This build (PR-R1P9)

- **Auto Run:** after rebase or when working tree is clean — staging and commit are **skipped** (log only), scenario continues to push.
- Includes PR-R1P8: autonomous rebase recovery, conflict resolve, branch detection during rebase.

## Notes

- Draft push: `git push origin HEAD:refs/heads/<name>`.
- Empty draft field on Run = normal push of current branch.
