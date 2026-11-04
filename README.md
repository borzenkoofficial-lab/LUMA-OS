# LUMA OS

A native Windows shell experience built with C# and WinUI 3.

## Project status

Early foundation stage. The repository is being initialized; no shell-replacement behavior is enabled yet.

## Planned architecture

- LUMA Shell — application host and navigation
- Desktop — desktop surface and wallpaper layer
- Dock — pinned and running applications
- Launchpad — searchable application launcher
- Spotlight — global command and search surface
- Control Center — quick system controls
- Windows Integration — isolated Win32 integration services

## Technology

- C# / .NET 10
- WinUI 3 / Windows App SDK
- Windows Composition APIs
- SQLite for local state (planned)

## Safety

LUMA will not disable or replace Windows Explorer until a tested recovery path, startup rollback, and safe-mode workflow exist. Early builds are development previews, not production shell replacements.

## Build

The solution and Windows build workflow will be added during the foundation milestone.
