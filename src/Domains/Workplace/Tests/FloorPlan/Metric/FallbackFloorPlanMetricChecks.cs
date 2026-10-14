namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class FallbackFloorPlanMetricChecks
{
    public static void FallbackFloorPlanMetricStartsInIdle()
    {
        var item = FallbackFloorPlanMetric.NewFallbackFloorPlanMetric("code-11", "label-899");
        Check.Equal(item.Status, FallbackFloorPlanMetricStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackFloorPlanMetricMovesToWarming()
    {
        var item = FallbackFloorPlanMetric.NewFallbackFloorPlanMetric("code-11", "label-899");
        try
        {
            item.MoveTo(FallbackFloorPlanMetricStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackFloorPlanMetricStatus.Warming, "status");
    }

    public static void FallbackFloorPlanMetricRejectsMovesFromStopped()
    {
        var item = FallbackFloorPlanMetric.NewFallbackFloorPlanMetric("code-11", "label-899");
        item.Status = FallbackFloorPlanMetricStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackFloorPlanMetricStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackFloorPlanMetricStartsInIdle();
        FallbackFloorPlanMetricMovesToWarming();
        FallbackFloorPlanMetricRejectsMovesFromStopped();
    }
}
