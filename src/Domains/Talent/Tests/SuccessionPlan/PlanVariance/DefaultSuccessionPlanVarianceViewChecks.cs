namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanVariance;

using Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

/// <summary>Checks of plan variance code, run by the project's runner.</summary>
public static class DefaultSuccessionPlanVarianceViewChecks
{
    public static void DefaultSuccessionPlanVarianceViewShowsTheStatusInCapitals()
    {
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("code-263", "title-319");
        var view = DefaultSuccessionPlanVarianceView.ToDefaultSuccessionPlanVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultSuccessionPlanVarianceView.ToDefaultSuccessionPlanVarianceViews(new List<DefaultSuccessionPlanVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultSuccessionPlanVarianceViewShowsTheStatusInCapitals();
    }
}
