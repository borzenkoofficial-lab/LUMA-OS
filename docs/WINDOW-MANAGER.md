# LUMA Window Manager

## Scope

The Window Manager is an opt-in control surface inside the LUMA application. It enumerates visible top-level Windows windows and exposes three explicit actions:

- Focus: restore a minimized window and request foreground activation.
- Minimize: minimize the selected window.
- Close: send the standard WM_CLOSE message.

## Safety boundaries

- LUMA excludes its own host window from the list.
- LUMA does not terminate processes and does not use TerminateProcess.
- Close is a request, not a forced kill. The target application remains responsible for save prompts and cancellation.
- Windows may deny foreground activation due to the operating system's foreground-lock rules. LUMA reports that result rather than claiming success.
- This feature does not replace Explorer, change the Windows shell registry, or alter startup behavior.

## Current limitations

- The list includes visible top-level windows with a non-empty title.
- Packaged application windows are included when Windows exposes them through standard top-level window APIs.
- Window grouping, thumbnails, virtual desktop assignment, snapping, and multi-monitor geometry are not implemented yet.
