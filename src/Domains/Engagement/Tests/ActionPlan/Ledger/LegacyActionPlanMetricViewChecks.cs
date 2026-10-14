namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LegacyActionPlanMetricViewChecks
{
    public static void LegacyActionPlanMetricViewShowsTheStatusInCapitals()
    {
        var item = LegacyActionPlanMetric.NewLegacyActionPlanMetric("code-891", "label-979");
        var view = LegacyActionPlanMetricView.ToLegacyActionPlanMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyActionPlanMetricView.ToLegacyActionPlanMetricViews(new List<LegacyActionPlanMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyActionPlanMetricViewShowsTheStatusInCapitals();
    }
}
