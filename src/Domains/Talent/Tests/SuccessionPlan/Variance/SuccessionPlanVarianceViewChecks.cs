namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanVarianceViewChecks
{
    public static void SuccessionPlanVarianceViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanVariance.NewSuccessionPlanVariance("code-346", "name-173");
        var view = SuccessionPlanVarianceView.ToSuccessionPlanVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanVarianceView.ToSuccessionPlanVarianceViews(new List<SuccessionPlanVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanVarianceViewShowsTheStatusInCapitals();
    }
}
