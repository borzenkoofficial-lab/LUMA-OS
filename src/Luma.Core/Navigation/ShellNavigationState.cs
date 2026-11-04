namespace Luma.Core.Navigation;

public sealed class ShellNavigationState
{
    public ShellSection CurrentSection { get; private set; } = ShellSection.Home;

    public void NavigateTo(ShellSection section)
    {
        if (!Enum.IsDefined(section))
            throw new ArgumentOutOfRangeException(nameof(section), section, "Unknown shell section.");

        CurrentSection = section;
    }
}
