namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanClaimViewChecks
{
    public static void FloorPlanClaimViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanClaim.NewFloorPlanClaim("code-445", "label-512");
        var view = FloorPlanClaimView.ToFloorPlanClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanClaimView.ToFloorPlanClaimViews(new List<FloorPlanClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanClaimViewShowsTheStatusInCapitals();
    }
}
