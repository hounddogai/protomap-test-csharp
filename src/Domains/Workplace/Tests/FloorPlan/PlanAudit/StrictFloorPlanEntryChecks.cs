namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class StrictFloorPlanEntryChecks
{
    public static void StrictFloorPlanEntryStartsInRequested()
    {
        var item = StrictFloorPlanEntry.NewStrictFloorPlanEntry("code-550", "name-584");
        Check.Equal(item.Status, StrictFloorPlanEntryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictFloorPlanEntryMovesToGranted()
    {
        var item = StrictFloorPlanEntry.NewStrictFloorPlanEntry("code-550", "name-584");
        try
        {
            item.MoveTo(StrictFloorPlanEntryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictFloorPlanEntryStatus.Granted, "status");
    }

    public static void StrictFloorPlanEntryRejectsMovesFromExpired()
    {
        var item = StrictFloorPlanEntry.NewStrictFloorPlanEntry("code-550", "name-584");
        item.Status = StrictFloorPlanEntryStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictFloorPlanEntryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictFloorPlanEntryStartsInRequested();
        StrictFloorPlanEntryMovesToGranted();
        StrictFloorPlanEntryRejectsMovesFromExpired();
    }
}
