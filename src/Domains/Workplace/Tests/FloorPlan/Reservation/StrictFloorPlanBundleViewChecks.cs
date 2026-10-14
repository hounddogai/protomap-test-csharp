namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class StrictFloorPlanBundleViewChecks
{
    public static void StrictFloorPlanBundleViewShowsTheStatusInCapitals()
    {
        var item = StrictFloorPlanBundle.NewStrictFloorPlanBundle("code-528", "name-424");
        var view = StrictFloorPlanBundleView.ToStrictFloorPlanBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictFloorPlanBundleView.ToStrictFloorPlanBundleViews(new List<StrictFloorPlanBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictFloorPlanBundleViewShowsTheStatusInCapitals();
    }
}
