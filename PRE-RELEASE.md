# Pre-release — WPF Auto Git Helper

**Version:** `1.5.0 Build PR-R1P10`  
**Date:** 2026-05-19  
**Channel:** `GITHUB local\pre-release\WpfAutoGitHelper`  
**Target GitHub release:** 1.5.0 (public site still 1.4.4 until release)

## Build

- **Exe:** `WpfAutoGitHelper\bin\Release\WpfAutoGitHelper.exe`
- **Framework:** .NET Framework 4.8 (WPF)

## This build (PR-R1P10)

- **Auto release:** `gh release create --target` uses a branch that **exists on origin** (upstream / draft name / main), not only local `development-test`.
- Includes PR-R1P9: skip commit when tree clean; PR-R1P8: autonomous rebase recovery.

## Notes

- After draft push to `dev-test`, release targets `dev-test` on GitHub automatically.
- Mark **Prerelease** on the Release tab for pre-release builds.
