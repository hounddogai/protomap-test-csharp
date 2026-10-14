namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopTierViewChecks
{
    public static void LaptopTierViewShowsTheStatusInCapitals()
    {
        var item = LaptopTier.NewLaptopTier("code-469", "title-319");
        var view = LaptopTierView.ToLaptopTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopTierView.ToLaptopTierViews(new List<LaptopTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopTierViewShowsTheStatusInCapitals();
    }
}
