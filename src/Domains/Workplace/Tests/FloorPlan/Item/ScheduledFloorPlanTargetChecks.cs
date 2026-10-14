namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class ScheduledFloorPlanTargetChecks
{
    public static void ScheduledFloorPlanTargetStartsInCreated()
    {
        var item = ScheduledFloorPlanTarget.NewScheduledFloorPlanTarget("code-715", "label-187");
        Check.Equal(item.Status, ScheduledFloorPlanTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledFloorPlanTargetMovesToRetired()
    {
        var item = ScheduledFloorPlanTarget.NewScheduledFloorPlanTarget("code-715", "label-187");
        try
        {
            item.MoveTo(ScheduledFloorPlanTargetStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledFloorPlanTargetStatus.Retired, "status");
    }

    public static void ScheduledFloorPlanTargetRejectsMovesFromRetired()
    {
        var item = ScheduledFloorPlanTarget.NewScheduledFloorPlanTarget("code-715", "label-187");
        item.Status = ScheduledFloorPlanTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledFloorPlanTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanTargetStartsInCreated();
        ScheduledFloorPlanTargetMovesToRetired();
        ScheduledFloorPlanTargetRejectsMovesFromRetired();
    }
}
