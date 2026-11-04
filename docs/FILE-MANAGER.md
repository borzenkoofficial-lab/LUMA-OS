# LUMA File Manager

## Current capabilities

- Navigate to a typed folder path or choose a folder with the Windows folder picker.
- Move to the parent directory.
- List immediate subfolders and files, with folder/file type, size, and modification time.
- Open folders inside LUMA and open files through their Windows default application.

## Safety

This milestone is intentionally read-only. It does not expose delete, rename, move, copy, overwrite, or permission-changing actions. It does not run elevated and does not bypass Windows access controls. Directory enumeration is limited to one level at a time; the browser does not recursively crawl the disk.

## Limitations

- No thumbnails, preview pane, tabs, search indexing, network share management, archive browsing, or recycle-bin integration yet.
- Access-denied entries may be omitted from a folder listing.
