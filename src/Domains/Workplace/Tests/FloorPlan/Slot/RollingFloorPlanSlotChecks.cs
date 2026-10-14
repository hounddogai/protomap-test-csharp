namespace Acme.Workforce.Workplace.Tests.FloorPlan.Slot;

using Acme.Workforce.Workplace.FloorPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class RollingFloorPlanSlotChecks
{
    public static void RollingFloorPlanSlotStartsInPlanned()
    {
        var item = RollingFloorPlanSlot.NewRollingFloorPlanSlot("code-924", "title-49");
        Check.Equal(item.Status, RollingFloorPlanSlotStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingFloorPlanSlotMovesToCompleted()
    {
        var item = RollingFloorPlanSlot.NewRollingFloorPlanSlot("code-924", "title-49");
        try
        {
            item.MoveTo(RollingFloorPlanSlotStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingFloorPlanSlotStatus.Completed, "status");
    }

    public static void RollingFloorPlanSlotRejectsMovesFromCancelled()
    {
        var item = RollingFloorPlanSlot.NewRollingFloorPlanSlot("code-924", "title-49");
        item.Status = RollingFloorPlanSlotStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingFloorPlanSlotStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingFloorPlanSlotStartsInPlanned();
        RollingFloorPlanSlotMovesToCompleted();
        RollingFloorPlanSlotRejectsMovesFromCancelled();
    }
}
