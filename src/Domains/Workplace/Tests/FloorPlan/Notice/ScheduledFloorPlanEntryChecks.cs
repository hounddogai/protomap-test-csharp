namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledFloorPlanEntryChecks
{
    public static void ScheduledFloorPlanEntryStartsInRequested()
    {
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("code-708", "title-367");
        Check.Equal(item.Status, ScheduledFloorPlanEntryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledFloorPlanEntryMovesToGranted()
    {
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("code-708", "title-367");
        try
        {
            item.MoveTo(ScheduledFloorPlanEntryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledFloorPlanEntryStatus.Granted, "status");
    }

    public static void ScheduledFloorPlanEntryRejectsMovesFromExpired()
    {
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("code-708", "title-367");
        item.Status = ScheduledFloorPlanEntryStatus.Expired;
        Check.Fails(() => item.MoveTo(ScheduledFloorPlanEntryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanEntryStartsInRequested();
        ScheduledFloorPlanEntryMovesToGranted();
        ScheduledFloorPlanEntryRejectsMovesFromExpired();
    }
}
