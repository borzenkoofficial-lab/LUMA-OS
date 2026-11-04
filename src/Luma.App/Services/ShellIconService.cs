using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Luma.App.Services;

public static class ShellIconService
{
    // Request a larger source thumbnail than the rendered Dock/Launchpad size.
    // Windows can then downsample it cleanly on high-DPI displays instead of
    // stretching a small 96px thumbnail across a 48–64px visual surface.
    private const uint ThumbnailSize = 256;

    public static async Task<ImageSource?> LoadShortcutIconAsync(string shortcutPath)
    {
        if (string.IsNullOrWhiteSpace(shortcutPath) || !File.Exists(shortcutPath))
            return null;

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(shortcutPath);
            using var thumbnail = await file.GetThumbnailAsync(
                ThumbnailMode.SingleItem,
                ThumbnailSize,
                ThumbnailOptions.ResizeThumbnail);

            if (thumbnail is null || thumbnail.Size == 0)
                return null;

            var image = new BitmapImage();
            await image.SetSourceAsync(thumbnail);
            return image;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
