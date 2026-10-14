namespace Acme.Workforce.Workplace.Tests.FloorPlan.Cycle;

using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanMetricChecks
{
    public static void AdaptiveFloorPlanMetricStartsInNew()
    {
        var item = AdaptiveFloorPlanMetric.NewAdaptiveFloorPlanMetric("code-156", "label-15");
        Check.Equal(item.Status, AdaptiveFloorPlanMetricStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveFloorPlanMetricMovesToRunning()
    {
        var item = AdaptiveFloorPlanMetric.NewAdaptiveFloorPlanMetric("code-156", "label-15");
        try
        {
            item.MoveTo(AdaptiveFloorPlanMetricStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveFloorPlanMetricStatus.Running, "status");
    }

    public static void AdaptiveFloorPlanMetricRejectsMovesFromFailed()
    {
        var item = AdaptiveFloorPlanMetric.NewAdaptiveFloorPlanMetric("code-156", "label-15");
        item.Status = AdaptiveFloorPlanMetricStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveFloorPlanMetricStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveFloorPlanMetricStartsInNew();
        AdaptiveFloorPlanMetricMovesToRunning();
        AdaptiveFloorPlanMetricRejectsMovesFromFailed();
    }
}
