namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanSummaryViewChecks
{
    public static void RegionalSuccessionPlanSummaryViewShowsTheStatusInCapitals()
    {
        var item = RegionalSuccessionPlanSummary.NewRegionalSuccessionPlanSummary("code-426", "label-811");
        var view = RegionalSuccessionPlanSummaryView.ToRegionalSuccessionPlanSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalSuccessionPlanSummaryView.ToRegionalSuccessionPlanSummaryViews(new List<RegionalSuccessionPlanSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalSuccessionPlanSummaryViewShowsTheStatusInCapitals();
    }
}
