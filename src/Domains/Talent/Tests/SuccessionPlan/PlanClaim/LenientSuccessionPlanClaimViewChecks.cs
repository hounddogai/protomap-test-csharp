namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanClaim;

using Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>Checks of plan claim code, run by the project's runner.</summary>
public static class LenientSuccessionPlanClaimViewChecks
{
    public static void LenientSuccessionPlanClaimViewShowsTheStatusInCapitals()
    {
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("code-597", "label-734");
        var view = LenientSuccessionPlanClaimView.ToLenientSuccessionPlanClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientSuccessionPlanClaimView.ToLenientSuccessionPlanClaimViews(new List<LenientSuccessionPlanClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanClaimViewShowsTheStatusInCapitals();
    }
}
