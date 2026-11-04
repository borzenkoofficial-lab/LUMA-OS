# LUMA Window Snapping

LUMA uses the nearest monitor's Windows work area, not its full physical bounds. This keeps snapped windows clear of the taskbar and respects secondary-monitor offsets, including monitors positioned left of or above the primary display.

Supported layouts in the core geometry engine:
- Left and right halves.
- Top-left, top-right, bottom-left, and bottom-right quadrants.

The UI currently exposes left half, right half, and maximize actions. The geometry engine is independent of Win32 and is covered by unit tests, including odd-sized work areas and negative monitor coordinates.

LUMA restores a window before positioning it, then uses SetWindowPos without changing Z-order or forcing activation. It does not change window ownership, process state, or Explorer.
