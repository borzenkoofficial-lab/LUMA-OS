# LUMA roadmap

The roadmap is organized around transforming the user's actual Windows 11 desktop—not building a separate desktop imitation.

## M0 — Architecture correction
- [x] Define LUMA as a Windows shell transformation layer
- [x] Define supported Windows surfaces and safety boundaries
- [ ] Refactor current preview prototype into the new runtime architecture
- [ ] Add Windows 11 compatibility detection and capability reporting

## M1 — First usable Windows makeover
- [ ] Per-user installer with preview/consent
- [ ] Backup original values before applying any customization
- [ ] LUMA Dock overlay with pinned and running Windows apps
- [ ] Launchpad for installed Windows applications
- [ ] Spotlight for apps, files, settings and commands
- [ ] LUMA wallpaper, theme palette and shortcut icon pack
- [ ] Optional startup at sign-in
- [ ] Recovery mode, disable switch, rollback and clean uninstall
- [ ] Keep Explorer and native taskbar available as recovery

## M2 — Desktop and system surfaces
- [ ] LUMA desktop layer and desktop icon rendering
- [ ] Control Center backed by supported Windows APIs
- [ ] Widgets and workspace interactions
- [ ] Native window management and animation layer
- [ ] Expand supported Windows appearance integration

## M3 — Optional shell replacement
- [ ] Per-user shell replacement mode, explicitly opt-in
- [ ] Startup rollback and Explorer recovery validated on supported Windows 11 builds
- [ ] Safe mode and crash-loop detection
- [ ] Integration, upgrade, uninstall and recovery tests
- [ ] Never replace login, UAC, security or protected OS surfaces

## M4 — Deep visual coverage
- [ ] Extend LUMA design to remaining supported Windows shell surfaces
- [ ] Version-specific compatibility adapters
- [ ] Performance, accessibility and multi-monitor validation

## Release gates

A feature is not complete because it looks correct inside the LUMA preview window. It must change the corresponding Windows surface, report whether the operation succeeded, and have a tested rollback path.

Explorer replacement is prohibited until recovery, safe mode, startup rollback and compatibility tests pass.
