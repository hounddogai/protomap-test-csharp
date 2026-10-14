namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class RollingRateSlotViewChecks
{
    public static void RollingRateSlotViewShowsTheStatusInCapitals()
    {
        var item = RollingRateSlot.NewRollingRateSlot("code-357", "label-916");
        var view = RollingRateSlotView.ToRollingRateSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingRateSlotView.ToRollingRateSlotViews(new List<RollingRateSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingRateSlotViewShowsTheStatusInCapitals();
    }
}
