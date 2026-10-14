namespace Acme.Workforce.Workplace.Tests.FloorPlan.Bundle;

using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FloorPlanRequestViewChecks
{
    public static void FloorPlanRequestViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanRequest.NewFloorPlanRequest("code-524", "label-861");
        var view = FloorPlanRequestView.ToFloorPlanRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanRequestView.ToFloorPlanRequestViews(new List<FloorPlanRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanRequestViewShowsTheStatusInCapitals();
    }
}
