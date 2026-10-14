namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class StrictFloorPlanEntryViewChecks
{
    public static void StrictFloorPlanEntryViewShowsTheStatusInCapitals()
    {
        var item = StrictFloorPlanEntry.NewStrictFloorPlanEntry("code-550", "name-584");
        var view = StrictFloorPlanEntryView.ToStrictFloorPlanEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictFloorPlanEntryView.ToStrictFloorPlanEntryViews(new List<StrictFloorPlanEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictFloorPlanEntryViewShowsTheStatusInCapitals();
    }
}
