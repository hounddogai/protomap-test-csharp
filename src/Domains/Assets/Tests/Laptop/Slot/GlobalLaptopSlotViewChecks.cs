namespace Acme.Workforce.Assets.Tests.Laptop.Slot;

using Acme.Workforce.Assets.Laptop.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class GlobalLaptopSlotViewChecks
{
    public static void GlobalLaptopSlotViewShowsTheStatusInCapitals()
    {
        var item = GlobalLaptopSlot.NewGlobalLaptopSlot("code-624", "name-563");
        var view = GlobalLaptopSlotView.ToGlobalLaptopSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalLaptopSlotView.ToGlobalLaptopSlotViews(new List<GlobalLaptopSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalLaptopSlotViewShowsTheStatusInCapitals();
    }
}
