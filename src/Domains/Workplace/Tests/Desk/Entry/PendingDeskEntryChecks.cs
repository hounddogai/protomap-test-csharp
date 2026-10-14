namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PendingDeskEntryChecks
{
    public static void PendingDeskEntryStartsInIdle()
    {
        var item = PendingDeskEntry.NewPendingDeskEntry("code-230", "label-39");
        Check.Equal(item.Status, PendingDeskEntryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingDeskEntryMovesToStopped()
    {
        var item = PendingDeskEntry.NewPendingDeskEntry("code-230", "label-39");
        try
        {
            item.MoveTo(PendingDeskEntryStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingDeskEntryStatus.Stopped, "status");
    }

    public static void PendingDeskEntryRejectsMovesFromStopped()
    {
        var item = PendingDeskEntry.NewPendingDeskEntry("code-230", "label-39");
        item.Status = PendingDeskEntryStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingDeskEntryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingDeskEntryStartsInIdle();
        PendingDeskEntryMovesToStopped();
        PendingDeskEntryRejectsMovesFromStopped();
    }
}
