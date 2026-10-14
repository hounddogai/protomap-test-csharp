namespace Acme.Workforce.Assets.Tests.Laptop.Baseline;

using Acme.Workforce.Assets.Laptop.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LaptopSlotViewChecks
{
    public static void LaptopSlotViewShowsTheStatusInCapitals()
    {
        var item = LaptopSlot.NewLaptopSlot("code-230", "title-118");
        var view = LaptopSlotView.ToLaptopSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopSlotView.ToLaptopSlotViews(new List<LaptopSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopSlotViewShowsTheStatusInCapitals();
    }
}
