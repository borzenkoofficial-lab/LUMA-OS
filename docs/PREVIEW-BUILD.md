# LUMA Preview Build

The Windows CI workflow publishes a temporary **LUMA-OS-preview** artifact after a successful Restore, Build, and Core Tests run.

## Download

1. Open the repository's [Actions](https://github.com/borzenkoofficial-lab/LUMA-OS/actions).
2. Open the latest successful **Windows CI** run on `main`.
3. In the Artifacts section, download `LUMA-OS-preview-<commit>`.
4. Extract the ZIP and run `Luma.App.exe`.

## Requirements and limitations

- Windows 10 version 1809 (build 17763) or newer; Windows 11 recommended.
- This is an unpackaged development preview, not an installer.
- The Microsoft Windows App Runtime matching Windows App SDK 2.6 must be installed on the PC. If Windows reports a missing Windows App Runtime, install the official Microsoft Windows App Runtime 1.8 (x64) and retry.
- Run LUMA as a normal user. Do not replace Explorer or change the Windows shell registry.
- The preview is for evaluating the current LUMA workspace UI and basic interactions. Some system integrations are not implemented yet.
- Artifacts are retained for 14 days. The final installer will be prepared separately after shell recovery and deployment safety are complete.
