namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LaptopCycleViewChecks
{
    public static void LaptopCycleViewShowsTheStatusInCapitals()
    {
        var item = LaptopCycle.NewLaptopCycle("code-221", "name-183");
        var view = LaptopCycleView.ToLaptopCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopCycleView.ToLaptopCycleViews(new List<LaptopCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopCycleViewShowsTheStatusInCapitals();
    }
}
