namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualFloorPlanMetricChecks
{
    public static void ManualFloorPlanMetricStartsInCreated()
    {
        var item = ManualFloorPlanMetric.NewManualFloorPlanMetric("code-45", "label-706");
        Check.Equal(item.Status, ManualFloorPlanMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualFloorPlanMetricMovesToValidated()
    {
        var item = ManualFloorPlanMetric.NewManualFloorPlanMetric("code-45", "label-706");
        try
        {
            item.MoveTo(ManualFloorPlanMetricStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualFloorPlanMetricStatus.Validated, "status");
    }

    public static void ManualFloorPlanMetricRejectsMovesFromRetired()
    {
        var item = ManualFloorPlanMetric.NewManualFloorPlanMetric("code-45", "label-706");
        item.Status = ManualFloorPlanMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualFloorPlanMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualFloorPlanMetricStartsInCreated();
        ManualFloorPlanMetricMovesToValidated();
        ManualFloorPlanMetricRejectsMovesFromRetired();
    }
}
