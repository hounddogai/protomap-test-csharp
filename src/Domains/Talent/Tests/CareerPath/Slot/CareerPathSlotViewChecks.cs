namespace Acme.Workforce.Talent.Tests.CareerPath.Slot;

using Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CareerPathSlotViewChecks
{
    public static void CareerPathSlotViewShowsTheStatusInCapitals()
    {
        var item = CareerPathSlot.NewCareerPathSlot("code-328", "title-716");
        var view = CareerPathSlotView.ToCareerPathSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathSlotView.ToCareerPathSlotViews(new List<CareerPathSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathSlotViewShowsTheStatusInCapitals();
    }
}
