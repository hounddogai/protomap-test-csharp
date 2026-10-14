namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledFloorPlanEntryViewChecks
{
    public static void ScheduledFloorPlanEntryViewShowsTheStatusInCapitals()
    {
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("code-708", "title-367");
        var view = ScheduledFloorPlanEntryView.ToScheduledFloorPlanEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledFloorPlanEntryView.ToScheduledFloorPlanEntryViews(new List<ScheduledFloorPlanEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanEntryViewShowsTheStatusInCapitals();
    }
}
