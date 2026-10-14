namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Quota;

using Acme.Workforce.Talent.SuccessionPlan.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RollingSuccessionPlanRequestViewChecks
{
    public static void RollingSuccessionPlanRequestViewShowsTheStatusInCapitals()
    {
        var item = RollingSuccessionPlanRequest.NewRollingSuccessionPlanRequest("code-368", "name-342");
        var view = RollingSuccessionPlanRequestView.ToRollingSuccessionPlanRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingSuccessionPlanRequestView.ToRollingSuccessionPlanRequestViews(new List<RollingSuccessionPlanRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanRequestViewShowsTheStatusInCapitals();
    }
}
