# WindowsAppSDK 1.8 × MAUI 9.0.x MSB3030 — reproduction

Branch: `windowsSDk-maui9-issue` (off `master`). Repo: `jasells/libVlcSharp.Maui.Samples`.

## Goal

Reproduce the **MSB3030 "Could not copy the file … HybridWebView.js"** build failure
([WindowsAppSDK#6032](https://github.com/microsoft/WindowsAppSDK/issues/6032)) using **official
NuGet packages**, so we can later validate the upstream WinUI fix (per-TFM WindowsAppSDK pin,
documented in `libvlcsharp/windowsSdk-maui9-fix.md`).

## Root-cause hypothesis (confirmed below)

Official `LibVLCSharp.MAUI 3.10.0` → `LibVLCSharp.WinUI 3.10.0` → `Microsoft.WindowsAppSDK 1.8.251106002`.
WindowsAppSDK 1.8's build targets try to copy `Microsoft.Maui/Handlers/HybridWebView/HybridWebView.js`
from the consuming MAUI package, but MAUI **9.0.120** does not ship that file at that path → MSB3030.

## Changes made (this branch)

`LibVLCSharp.MAUI.Sample/LibVLCSharp.MAUI.Sample.csproj`:
- Bumped TFMs `net8.0-*` → `net9.0-*` (kept the standard multi-target MAUI shape:
  `net9.0-android;net9.0-ios` + windows-conditional `net9.0-windows10.0.19041.0`).
- Replaced the local `ProjectReference` to `..\LibVLCSharp.MAUI\LibVLCSharp.MAUI.csproj` with a
  `PackageReference` on **official `LibVLCSharp.MAUI 3.10.0`** (this is what transitively pulls
  WindowsAppSDK 1.8).
- Bumped `Microsoft.Maui.Controls` `8.0.70` → **`9.0.120`** (the version in the reported repro).
- Kept the three platform `VideoLAN.LibVLC.*` refs — the app stays multi-target / realistic.

Added `global.json` at the repo root pinning the .NET 9 SDK (`9.0.311`, `rollForward: latestFeature`).

## Repro command

```powershell
cd C:\Source\Repos\libVlc.Maui.Droid.Repro
dotnet build LibVLCSharp.MAUI.Sample\LibVLCSharp.MAUI.Sample.csproj -c Debug -f net9.0-windows10.0.19041.0
```

Environment: SDK `9.0.311`; workload `maui-windows 9.0.120` (android/ios/maccatalyst also installed).

## Result — REPRODUCED ✅

Build FAILED (2 errors), app code compiled clean against official 3.10.0 (warnings only):

```
error MSB3030: Could not copy the file
"C:\Users\josh\.nuget\packages\microsoft.maui.controls.core\9.0.120\lib\net9.0-windows10.0.19041\Microsoft.Maui\Handlers\HybridWebView\HybridWebView.js"
because it was not found.
error MSB3030: Could not copy the file
"C:\Users\josh\.nuget\packages\microsoft.maui.core\9.0.120\lib\net9.0-windows10.0.19041\Microsoft.Maui\Handlers\HybridWebView\HybridWebView.js"
because it was not found.
```
(from `...\sdk\9.0.311\Microsoft.Common.CurrentVersion.targets(5394,5)`)

Resolved chain in `obj/project.assets.json` (net9.0-windows target) — exactly as predicted:
- `LibVLCSharp.MAUI/3.10.0`
- `LibVLCSharp.WinUI/3.10.0`
- `Microsoft.WindowsAppSDK/1.8.251106002`
- `Microsoft.Maui.Controls/9.0.120`

## Next step (fix validation — separate work)

Build a patched `LibVLCSharp.WinUI` locally with the per-TFM WindowsAppSDK pin from
`libvlcsharp/windowsSdk-maui9-fix.md` (net9-windows → 1.7.260224002), publish to a local NuGet feed,
and repoint this sample at it (a `Debug`/`Debug-Fork`-style config toggle, as the #659 branch uses).
The Windows head should then build clean.

## Notes

- Prior in-progress #659 work was stashed (`git stash` on `repro-objectdisposed-detach-659`) before
  branching; recover it with `git stash pop` on that branch.
