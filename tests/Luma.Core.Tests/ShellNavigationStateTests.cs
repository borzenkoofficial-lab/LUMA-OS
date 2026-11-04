using Luma.Core.Navigation;

namespace Luma.Core.Tests;

public sealed class ShellNavigationStateTests
{
    [Fact]
    public void StartsOnHome()
    {
        var state = new ShellNavigationState();

        Assert.Equal(ShellSection.Home, state.CurrentSection);
    }

    [Theory]
    [InlineData(ShellSection.Applications)]
    [InlineData(ShellSection.Widgets)]
    [InlineData(ShellSection.Settings)]
    public void NavigatesToRequestedSection(ShellSection section)
    {
        var state = new ShellNavigationState();

        state.NavigateTo(section);

        Assert.Equal(section, state.CurrentSection);
    }

    [Fact]
    public void RejectsUndefinedSection()
    {
        var state = new ShellNavigationState();

        Assert.Throws<ArgumentOutOfRangeException>(() => state.NavigateTo((ShellSection)999));
    }
}
