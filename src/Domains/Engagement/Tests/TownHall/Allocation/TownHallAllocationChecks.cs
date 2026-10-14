namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallAllocationChecks
{
    public static void TownHallAllocationStartsInPlanned()
    {
        var item = TownHallAllocation.NewTownHallAllocation("code-719", "name-413");
        Check.Equal(item.Status, TownHallAllocationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallAllocationMovesToScheduled()
    {
        var item = TownHallAllocation.NewTownHallAllocation("code-719", "name-413");
        try
        {
            item.MoveTo(TownHallAllocationStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallAllocationStatus.Scheduled, "status");
    }

    public static void TownHallAllocationRejectsMovesFromCancelled()
    {
        var item = TownHallAllocation.NewTownHallAllocation("code-719", "name-413");
        item.Status = TownHallAllocationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TownHallAllocationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TownHallAllocationStartsInPlanned();
        TownHallAllocationMovesToScheduled();
        TownHallAllocationRejectsMovesFromCancelled();
    }
}
