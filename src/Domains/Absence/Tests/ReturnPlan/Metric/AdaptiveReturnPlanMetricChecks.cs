namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AdaptiveReturnPlanMetricChecks
{
    public static void AdaptiveReturnPlanMetricStartsInOpen()
    {
        var item = AdaptiveReturnPlanMetric.NewAdaptiveReturnPlanMetric("code-589", "title-185");
        Check.Equal(item.Status, AdaptiveReturnPlanMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveReturnPlanMetricMovesToResolved()
    {
        var item = AdaptiveReturnPlanMetric.NewAdaptiveReturnPlanMetric("code-589", "title-185");
        try
        {
            item.MoveTo(AdaptiveReturnPlanMetricStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveReturnPlanMetricStatus.Resolved, "status");
    }

    public static void AdaptiveReturnPlanMetricRejectsMovesFromArchived()
    {
        var item = AdaptiveReturnPlanMetric.NewAdaptiveReturnPlanMetric("code-589", "title-185");
        item.Status = AdaptiveReturnPlanMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveReturnPlanMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveReturnPlanMetricStartsInOpen();
        AdaptiveReturnPlanMetricMovesToResolved();
        AdaptiveReturnPlanMetricRejectsMovesFromArchived();
    }
}
