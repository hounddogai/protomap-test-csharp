namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SuccessionPlanScoreViewChecks
{
    public static void SuccessionPlanScoreViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanScore.NewSuccessionPlanScore("code-492", "title-253");
        var view = SuccessionPlanScoreView.ToSuccessionPlanScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanScoreView.ToSuccessionPlanScoreViews(new List<SuccessionPlanScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanScoreViewShowsTheStatusInCapitals();
    }
}
