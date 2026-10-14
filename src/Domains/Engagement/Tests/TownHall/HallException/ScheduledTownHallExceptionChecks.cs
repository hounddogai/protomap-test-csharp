namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class ScheduledTownHallExceptionChecks
{
    public static void ScheduledTownHallExceptionStartsInProposed()
    {
        var item = ScheduledTownHallException.NewScheduledTownHallException("code-218", "name-6");
        Check.Equal(item.Status, ScheduledTownHallExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledTownHallExceptionMovesToApproved()
    {
        var item = ScheduledTownHallException.NewScheduledTownHallException("code-218", "name-6");
        try
        {
            item.MoveTo(ScheduledTownHallExceptionStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledTownHallExceptionStatus.Approved, "status");
    }

    public static void ScheduledTownHallExceptionRejectsMovesFromWithdrawn()
    {
        var item = ScheduledTownHallException.NewScheduledTownHallException("code-218", "name-6");
        item.Status = ScheduledTownHallExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledTownHallExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledTownHallExceptionStartsInProposed();
        ScheduledTownHallExceptionMovesToApproved();
        ScheduledTownHallExceptionRejectsMovesFromWithdrawn();
    }
}
