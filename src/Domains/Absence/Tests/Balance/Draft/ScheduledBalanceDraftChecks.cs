namespace Acme.Workforce.Absence.Tests.Balance.Draft;

using Acme.Workforce.Absence.Balance.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledBalanceDraftChecks
{
    public static void ScheduledBalanceDraftStartsInRequested()
    {
        var item = ScheduledBalanceDraft.NewScheduledBalanceDraft("code-63", "name-491");
        Check.Equal(item.Status, ScheduledBalanceDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledBalanceDraftMovesToGranted()
    {
        var item = ScheduledBalanceDraft.NewScheduledBalanceDraft("code-63", "name-491");
        try
        {
            item.MoveTo(ScheduledBalanceDraftStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledBalanceDraftStatus.Granted, "status");
    }

    public static void ScheduledBalanceDraftRejectsMovesFromExpired()
    {
        var item = ScheduledBalanceDraft.NewScheduledBalanceDraft("code-63", "name-491");
        item.Status = ScheduledBalanceDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(ScheduledBalanceDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ScheduledBalanceDraftStartsInRequested();
        ScheduledBalanceDraftMovesToGranted();
        ScheduledBalanceDraftRejectsMovesFromExpired();
    }
}
