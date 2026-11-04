# LUMA — Windows shell transformation architecture

## Product definition

LUMA is a macOS-inspired desktop shell and customization layer for Windows 11. It is not an operating system, a virtual desktop, or a standalone workspace that imitates Windows.

Windows remains the host operating system: kernel, drivers, services, security model, NTFS, installed Win32/UWP applications, and native process execution remain intact. LUMA changes the desktop experience progressively, using supported Windows settings and APIs first, then replaceable shell surfaces where a safe recovery path exists.

## User promise

The first public installer should make a visible, reversible change to the user's existing Windows installation. The user installs one EXE, reviews the changes, confirms them, and gets a coherent LUMA desktop without reinstalling Windows or migrating files and applications.

Do not promise a complete macOS clone in the first release. Windows has protected and version-dependent shell surfaces. Every surface must have a documented implementation method, compatibility range, rollback, and fallback.

## Surface strategy

| Windows surface | LUMA approach | Initial release policy |
|---|---|---|
| Desktop wallpaper/background | Apply selected LUMA wallpaper through Windows personalization APIs/settings | Supported, reversible |
| Desktop shortcuts/icons | Create LUMA shortcut set and use per-shortcut icons where possible | Supported, reversible |
| System icons | Use supported per-user icon/customization mechanisms only; do not patch protected system resources | Limited; document gaps |
| Taskbar | Configure supported alignment/visibility options; provide LUMA Dock as the primary launcher | Do not inject into or patch Explorer |
| Dock | Native LUMA Dock overlay, centered at screen bottom, running/pinned apps, hover magnification, indicators | Core first-release feature |
| Start menu / Launchpad | LUMA Launchpad opened by explicit shortcut/hotkey; preserve native Start as recovery fallback | Core first-release feature |
| Search / Spotlight | LUMA Spotlight for apps, files, settings and commands; preserve Windows Search | Core first-release feature |
| Explorer / File dialogs | Keep Windows Explorer and native dialogs initially; apply supported theme settings and integrate LUMA navigation | Gradual |
| Window controls | Use native Windows window APIs for snapping, focus and workspace actions | Gradual |
| Quick settings / Control Center | LUMA panel backed by documented Windows APIs where available; open native Settings for unsupported controls | Gradual |
| Login, lock screen, UAC, security UI | Keep Windows-owned surfaces | Never replace in early milestones |
| System services, drivers, kernel | Windows-owned; LUMA does not replace or emulate them | Never replace |

## Runtime architecture

1. **Luma.Installer** — signed setup executable, prerequisite checks, install/update/uninstall, consent screen, create restore/recovery entry, register startup only after consent.
2. **Luma.Shell** — per-user desktop experience host; Dock, Launchpad, Spotlight, Control Center, wallpaper/theme coordination.
3. **Luma.WindowsIntegration** — isolated Windows API adapters. Each adapter reports capability, supported Windows builds, current value, apply result and rollback value.
4. **Luma.Settings** — local per-user configuration, profiles, theme assets, pinned apps and layout.
5. **Luma.Recovery** — safe mode, disable startup, restore previous settings, repair/uninstall. Must work even if the shell host crashes.
6. **Luma.Diagnostics** — compatibility report and logs with no collection/upload by default.

Keep Windows integration isolated from UI code. Do not use undocumented Explorer injection, binary patching, system DLL replacement, or registry changes without an explicit capability check and a tested rollback.

## Install and recovery contract

- Installation is per-user by default; request elevation only for a specific documented operation that requires it.
- Show a preview of every system setting that will change.
- Record the original value before changing any Windows setting.
- Never silently disable Explorer or remove the Windows taskbar.
- Register LUMA at sign-in only after explicit consent.
- Provide a startup recovery entry and a documented keyboard-accessible way to disable LUMA.
- If LUMA crashes, Windows Explorer remains available.
- Uninstall restores only values that LUMA changed and only when the current value still matches the value LUMA applied; do not overwrite later user changes.
- Installer and updater must be idempotent and support interrupted-install recovery.

## Delivery milestones

### M1 — Real Windows makeover (first usable release)
- LUMA installer and per-user setup.
- LUMA Dock with pinned and running Windows applications.
- Launchpad and Spotlight.
- LUMA wallpaper, theme palette, desktop shortcut/icon pack.
- Startup option with consent.
- Recovery/disable/uninstall and settings rollback.
- Explorer and native taskbar remain available.

### M2 — Shell surfaces
- LUMA desktop layer and desktop icon rendering.
- Control Center and widgets.
- Window/workspace interactions.
- More Windows appearance settings, each behind capability checks.

### M3 — Shell replacement mode (opt-in)
- Replace the Windows shell for the user session only after crash recovery, safe mode, sign-in recovery and Windows-version compatibility tests pass.
- Explorer remains the recovery shell.
- Explicit opt-in, preview, and one-click return to Explorer.

### M4 — Deep visual coverage
- Extend coverage to remaining supported shell surfaces.
- Do not replace login, UAC, security, or protected OS surfaces.
- Maintain compatibility per Windows 11 release.

## Engineering gates

No release may be called a Windows makeover until it actually changes the user's Windows desktop rather than only displaying a LUMA preview window.

Every Windows integration feature requires:
- Windows 11 build compatibility declaration.
- Automated unit tests for state/rollback logic.
- Integration test on a Windows runner or documented manual test where CI cannot access an interactive desktop.
- Failure handling and rollback test.
- No destructive changes to the user's data or system files.

## Current repository correction

The previous WinUI preview window is a prototype only. It is not the product architecture and must not be expanded as a standalone imitation desktop. Reuse isolated useful services and tests where appropriate, but rework the application around the installer, Windows integration adapters, real Dock/Launchpad/Spotlight surfaces, and recovery contract above.
