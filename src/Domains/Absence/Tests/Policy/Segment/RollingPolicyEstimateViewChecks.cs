namespace Acme.Workforce.Absence.Tests.Policy.Segment;

using Acme.Workforce.Absence.Policy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RollingPolicyEstimateViewChecks
{
    public static void RollingPolicyEstimateViewShowsTheStatusInCapitals()
    {
        var item = RollingPolicyEstimate.NewRollingPolicyEstimate("code-459", "title-587");
        var view = RollingPolicyEstimateView.ToRollingPolicyEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingPolicyEstimateView.ToRollingPolicyEstimateViews(new List<RollingPolicyEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingPolicyEstimateViewShowsTheStatusInCapitals();
    }
}
