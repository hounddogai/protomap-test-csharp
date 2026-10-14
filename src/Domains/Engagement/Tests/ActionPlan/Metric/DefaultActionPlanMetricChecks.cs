namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultActionPlanMetricChecks
{
    public static void DefaultActionPlanMetricStartsInProposed()
    {
        var item = DefaultActionPlanMetric.NewDefaultActionPlanMetric("code-827", "title-823");
        Check.Equal(item.Status, DefaultActionPlanMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultActionPlanMetricMovesToApproved()
    {
        var item = DefaultActionPlanMetric.NewDefaultActionPlanMetric("code-827", "title-823");
        try
        {
            item.MoveTo(DefaultActionPlanMetricStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultActionPlanMetricStatus.Approved, "status");
    }

    public static void DefaultActionPlanMetricRejectsMovesFromWithdrawn()
    {
        var item = DefaultActionPlanMetric.NewDefaultActionPlanMetric("code-827", "title-823");
        item.Status = DefaultActionPlanMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultActionPlanMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultActionPlanMetricStartsInProposed();
        DefaultActionPlanMetricMovesToApproved();
        DefaultActionPlanMetricRejectsMovesFromWithdrawn();
    }
}
