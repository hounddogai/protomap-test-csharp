namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanClaim;

using Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>Checks of plan claim code, run by the project's runner.</summary>
public static class LenientSuccessionPlanScoreViewChecks
{
    public static void LenientSuccessionPlanScoreViewShowsTheStatusInCapitals()
    {
        var item = LenientSuccessionPlanScore.NewLenientSuccessionPlanScore("code-332", "label-470");
        var view = LenientSuccessionPlanScoreView.ToLenientSuccessionPlanScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientSuccessionPlanScoreView.ToLenientSuccessionPlanScoreViews(new List<LenientSuccessionPlanScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanScoreViewShowsTheStatusInCapitals();
    }
}
