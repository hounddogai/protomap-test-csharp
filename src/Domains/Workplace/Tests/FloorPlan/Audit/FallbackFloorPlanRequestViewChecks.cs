namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackFloorPlanRequestViewChecks
{
    public static void FallbackFloorPlanRequestViewShowsTheStatusInCapitals()
    {
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("code-219", "label-774");
        var view = FallbackFloorPlanRequestView.ToFallbackFloorPlanRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackFloorPlanRequestView.ToFallbackFloorPlanRequestViews(new List<FallbackFloorPlanRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackFloorPlanRequestViewShowsTheStatusInCapitals();
    }
}
