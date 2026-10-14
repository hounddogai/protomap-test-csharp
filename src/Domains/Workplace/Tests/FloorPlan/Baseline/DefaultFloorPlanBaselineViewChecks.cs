namespace Acme.Workforce.Workplace.Tests.FloorPlan.Baseline;

using Acme.Workforce.Workplace.FloorPlan.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultFloorPlanBaselineViewChecks
{
    public static void DefaultFloorPlanBaselineViewShowsTheStatusInCapitals()
    {
        var item = DefaultFloorPlanBaseline.NewDefaultFloorPlanBaseline("code-376", "label-73");
        var view = DefaultFloorPlanBaselineView.ToDefaultFloorPlanBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultFloorPlanBaselineView.ToDefaultFloorPlanBaselineViews(new List<DefaultFloorPlanBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultFloorPlanBaselineViewShowsTheStatusInCapitals();
    }
}
