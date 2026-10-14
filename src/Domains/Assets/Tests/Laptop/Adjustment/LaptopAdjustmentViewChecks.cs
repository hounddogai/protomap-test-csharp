namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LaptopAdjustmentViewChecks
{
    public static void LaptopAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = LaptopAdjustment.NewLaptopAdjustment("code-881", "title-835");
        var view = LaptopAdjustmentView.ToLaptopAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopAdjustmentView.ToLaptopAdjustmentViews(new List<LaptopAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopAdjustmentViewShowsTheStatusInCapitals();
    }
}
