namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualSlotViewChecks
{
    public static void AccrualSlotViewShowsTheStatusInCapitals()
    {
        var item = AccrualSlot.NewAccrualSlot("code-110", "name-275");
        var view = AccrualSlotView.ToAccrualSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualSlotView.ToAccrualSlotViews(new List<AccrualSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualSlotViewShowsTheStatusInCapitals();
    }
}
