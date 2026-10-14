namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FloorPlanEntryChecks
{
    public static void FloorPlanEntryStartsInProposed()
    {
        var item = FloorPlanEntry.NewFloorPlanEntry("code-465", "name-482");
        Check.Equal(item.Status, FloorPlanEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanEntryMovesToRejected()
    {
        var item = FloorPlanEntry.NewFloorPlanEntry("code-465", "name-482");
        try
        {
            item.MoveTo(FloorPlanEntryStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanEntryStatus.Rejected, "status");
    }

    public static void FloorPlanEntryRejectsMovesFromWithdrawn()
    {
        var item = FloorPlanEntry.NewFloorPlanEntry("code-465", "name-482");
        item.Status = FloorPlanEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FloorPlanEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FloorPlanEntryStartsInProposed();
        FloorPlanEntryMovesToRejected();
        FloorPlanEntryRejectsMovesFromWithdrawn();
    }
}
