namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleSlotViewChecks
{
    public static void MeritCycleSlotViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleSlot.NewMeritCycleSlot("code-162", "title-761");
        var view = MeritCycleSlotView.ToMeritCycleSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleSlotView.ToMeritCycleSlotViews(new List<MeritCycleSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleSlotViewShowsTheStatusInCapitals();
    }
}
