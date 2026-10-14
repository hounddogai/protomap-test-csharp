namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedFloorPlanMetricChecks
{
    public static void WeightedFloorPlanMetricStartsInProposed()
    {
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("code-885", "label-710");
        Check.Equal(item.Status, WeightedFloorPlanMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedFloorPlanMetricMovesToWithdrawn()
    {
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("code-885", "label-710");
        try
        {
            item.MoveTo(WeightedFloorPlanMetricStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedFloorPlanMetricStatus.Withdrawn, "status");
    }

    public static void WeightedFloorPlanMetricRejectsMovesFromWithdrawn()
    {
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("code-885", "label-710");
        item.Status = WeightedFloorPlanMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedFloorPlanMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedFloorPlanMetricStartsInProposed();
        WeightedFloorPlanMetricMovesToWithdrawn();
        WeightedFloorPlanMetricRejectsMovesFromWithdrawn();
    }
}
