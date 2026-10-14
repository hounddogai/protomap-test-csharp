namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class AccrualDraftChecks
{
    public static void AccrualDraftStartsInIdle()
    {
        var item = AccrualDraft.NewAccrualDraft("code-437", "label-267");
        Check.Equal(item.Status, AccrualDraftStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualDraftMovesToReady()
    {
        var item = AccrualDraft.NewAccrualDraft("code-437", "label-267");
        try
        {
            item.MoveTo(AccrualDraftStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualDraftStatus.Ready, "status");
    }

    public static void AccrualDraftRejectsMovesFromStopped()
    {
        var item = AccrualDraft.NewAccrualDraft("code-437", "label-267");
        item.Status = AccrualDraftStatus.Stopped;
        Check.Fails(() => item.MoveTo(AccrualDraftStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AccrualDraftStartsInIdle();
        AccrualDraftMovesToReady();
        AccrualDraftRejectsMovesFromStopped();
    }
}
