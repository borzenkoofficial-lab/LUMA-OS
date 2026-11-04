# LUMA roadmap

## Milestone 0 — Foundation
- [x] Create repository and solution structure
- [x] Add native WinUI 3 application host
- [x] Add isolated core navigation state and unit tests
- [x] Add Windows CI workflow
- [ ] Verify Windows CI build and test results

## Milestone 1 — Desktop experience
- [ ] Desktop surface and wallpaper management
- [ ] Dock with pinned and running applications
- [ ] Keyboard-first Spotlight search
- [ ] Launchpad with application discovery

## Milestone 2 — System surfaces
- [ ] Control Center
- [ ] Widget host
- [ ] Theme and appearance preferences
- [ ] Window management

## Milestone 3 — Windows integration
- [ ] Isolated Win32 integration layer
- [ ] Startup registration with explicit user consent
- [ ] Explorer coexistence and rollback tests
- [ ] Recovery and safe-mode workflow

## Milestone 4 — Release
- [ ] Installer and update strategy
- [ ] Upgrade, uninstall, and recovery validation
- [ ] Performance and accessibility testing

## Safety gates
Explorer replacement is prohibited until startup rollback, recovery access, and a tested path back to the standard Windows shell are implemented. The application must remain usable as a normal desktop app before that gate is passed.
