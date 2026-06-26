# QVerisBot Windows Companion

First-stage QVerisBot-branded Windows companion app, based on
`openclaw/openclaw-windows-node@v0.6.3`.

This repository is intentionally separate from the main QVerisBot/OpenClaw
codebase so the Windows desktop packaging path can be validated without
disturbing the main project.

## Current milestone

- Produces unsigned Windows installers:
  - `QVerisBot-Setup-x64.exe`
  - `QVerisBot-Setup-arm64.exe`
  - `QVerisBot-SHA256SUMS.txt`
- Keeps the internal C# project names and namespaces from upstream for now.
- Keeps `openclaw://` deep-link compatibility for the first milestone.
- Prefers a `qverisbot` CLI when present and falls back to `openclaw`.
- Uses GitHub Releases from this repository for first-stage update metadata.

Unsigned installers are expected to show Windows security prompts. Code signing,
the `qverisbot://` protocol, and full QVerisBot gateway installer replacement are
planned follow-up work.

## Build locally

Prerequisites:

- Windows 10 20H2+ or Windows 11
- .NET 10 SDK
- Windows 10/11 SDK and WinUI build tooling
- WebView2 Runtime
- Inno Setup 6

Build and launch locally:

```powershell
.\build.ps1 -CheckOnly
.\build.ps1
.\run-app-local.ps1
```

Build a local unsigned installer:

```powershell
.\scripts\build-inno-local.ps1 -Arch x64 -Fast -Version 0.1.0
```

The local installer appears under `Output\`.

## GitHub release build

The first-stage release workflow is `.github/workflows/unsigned-release.yml`.

Recommended first release:

1. Push `main` to `QVerisAI/qverisbot-windows-companion`.
2. Create tag `v0.1.0`.
3. Push the tag.
4. Confirm the `Unsigned QVerisBot Release` workflow publishes:
   - `QVerisBot-Setup-x64.exe`
   - `QVerisBot-Setup-arm64.exe`
   - `QVerisBot-SHA256SUMS.txt`

The upstream Azure signing release job is disabled in this fork. The normal CI
workflow remains for build/test coverage.

## Compatibility notes

The current app still manages an OpenClaw-compatible gateway and state layout in
several places. That is deliberate for this stage: the objective is to validate
the desktop packaging and release path first.

Follow-up stages can migrate deeper surfaces:

- `qverisbot://` protocol registration
- QVerisBot-owned gateway install URL and npm package
- QVerisBot app data directories
- signed installers
- promotion of these installer assets into the main QVerisBot release

## Attribution

This repository is derived from
[`openclaw/openclaw-windows-node`](https://github.com/openclaw/openclaw-windows-node).
The first imported base was tag `v0.6.3`.
