namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class FloorPlanMetricChecks
{
    public static void FloorPlanMetricStartsInCreated()
    {
        var item = FloorPlanMetric.NewFloorPlanMetric("code-596", "name-36");
        Check.Equal(item.Status, FloorPlanMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanMetricMovesToRetired()
    {
        var item = FloorPlanMetric.NewFloorPlanMetric("code-596", "name-36");
        try
        {
            item.MoveTo(FloorPlanMetricStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanMetricStatus.Retired, "status");
    }

    public static void FloorPlanMetricRejectsMovesFromRetired()
    {
        var item = FloorPlanMetric.NewFloorPlanMetric("code-596", "name-36");
        item.Status = FloorPlanMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(FloorPlanMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        FloorPlanMetricStartsInCreated();
        FloorPlanMetricMovesToRetired();
        FloorPlanMetricRejectsMovesFromRetired();
    }
}
