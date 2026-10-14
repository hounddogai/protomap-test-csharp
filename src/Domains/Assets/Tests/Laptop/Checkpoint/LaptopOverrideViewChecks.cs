namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LaptopOverrideViewChecks
{
    public static void LaptopOverrideViewShowsTheStatusInCapitals()
    {
        var item = LaptopOverride.NewLaptopOverride("code-510", "title-261");
        var view = LaptopOverrideView.ToLaptopOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopOverrideView.ToLaptopOverrideViews(new List<LaptopOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopOverrideViewShowsTheStatusInCapitals();
    }
}
