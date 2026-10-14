namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class ManualFloorPlanItemChecks
{
    public static void ManualFloorPlanItemStartsInPlanned()
    {
        var item = ManualFloorPlanItem.NewManualFloorPlanItem("code-85", "title-904");
        Check.Equal(item.Status, ManualFloorPlanItemStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualFloorPlanItemMovesToScheduled()
    {
        var item = ManualFloorPlanItem.NewManualFloorPlanItem("code-85", "title-904");
        try
        {
            item.MoveTo(ManualFloorPlanItemStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualFloorPlanItemStatus.Scheduled, "status");
    }

    public static void ManualFloorPlanItemRejectsMovesFromCancelled()
    {
        var item = ManualFloorPlanItem.NewManualFloorPlanItem("code-85", "title-904");
        item.Status = ManualFloorPlanItemStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualFloorPlanItemStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualFloorPlanItemStartsInPlanned();
        ManualFloorPlanItemMovesToScheduled();
        ManualFloorPlanItemRejectsMovesFromCancelled();
    }
}
