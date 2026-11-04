# LUMA

**A macOS-inspired shell transformation for Windows 11.**

LUMA keeps Windows as the operating system and transforms the desktop experience: Dock, Launchpad, Spotlight, visual theme, wallpapers, desktop shortcuts, and—over time—more Windows shell surfaces.

LUMA is not a new operating system, a virtual machine, or a separate desktop that merely imitates Windows. Windows continues to provide the kernel, drivers, services, security, filesystem, and native application runtime.

## Product direction

- Install LUMA with one Windows installer.
- Preview and approve the changes before applying them.
- Transform the Windows desktop progressively, starting with reversible per-user changes.
- Keep Explorer and native Windows recovery available in the first release.
- Add opt-in shell replacement only after safe mode, rollback, and crash recovery are proven.

## First usable release

- Native LUMA Dock for pinned and running Windows apps.
- Launchpad and Spotlight.
- LUMA wallpapers, theme, and shortcut icon set.
- Per-user startup option with explicit consent.
- Recovery, disable, and uninstall with restoration of changed settings.

## Technology

- C# / .NET
- WinUI 3 / Windows App SDK for native Windows surfaces
- Windows APIs isolated behind integration adapters
- Per-user configuration and theme assets

## Safety

LUMA does not replace Windows, patch protected system binaries, disable security surfaces, or silently replace Explorer. Each applied setting must be recorded and reversible. Explorer remains available as a recovery path until shell-replacement mode passes dedicated safety gates.

## Architecture and roadmap

See [Windows shell transformation architecture](docs/WINDOWS-SHELL-ARCHITECTURE.md) and [roadmap](docs/ROADMAP.md).
