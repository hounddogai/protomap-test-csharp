namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TownHallCycleChecks
{
    public static void TownHallCycleStartsInProposed()
    {
        var item = TownHallCycle.NewTownHallCycle("code-767", "label-537");
        Check.Equal(item.Status, TownHallCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallCycleMovesToApproved()
    {
        var item = TownHallCycle.NewTownHallCycle("code-767", "label-537");
        try
        {
            item.MoveTo(TownHallCycleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallCycleStatus.Approved, "status");
    }

    public static void TownHallCycleRejectsMovesFromWithdrawn()
    {
        var item = TownHallCycle.NewTownHallCycle("code-767", "label-537");
        item.Status = TownHallCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TownHallCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TownHallCycleStartsInProposed();
        TownHallCycleMovesToApproved();
        TownHallCycleRejectsMovesFromWithdrawn();
    }
}
