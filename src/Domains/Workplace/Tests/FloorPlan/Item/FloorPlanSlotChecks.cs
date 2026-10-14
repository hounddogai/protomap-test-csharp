namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class FloorPlanSlotChecks
{
    public static void FloorPlanSlotStartsInCreated()
    {
        var item = FloorPlanSlot.NewFloorPlanSlot("code-745", "label-290");
        Check.Equal(item.Status, FloorPlanSlotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanSlotMovesToValidated()
    {
        var item = FloorPlanSlot.NewFloorPlanSlot("code-745", "label-290");
        try
        {
            item.MoveTo(FloorPlanSlotStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanSlotStatus.Validated, "status");
    }

    public static void FloorPlanSlotRejectsMovesFromRetired()
    {
        var item = FloorPlanSlot.NewFloorPlanSlot("code-745", "label-290");
        item.Status = FloorPlanSlotStatus.Retired;
        Check.Fails(() => item.MoveTo(FloorPlanSlotStatus.Created), "move");
    }

    public static void RunAll()
    {
        FloorPlanSlotStartsInCreated();
        FloorPlanSlotMovesToValidated();
        FloorPlanSlotRejectsMovesFromRetired();
    }
}
