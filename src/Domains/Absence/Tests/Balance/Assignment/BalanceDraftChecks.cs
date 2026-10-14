namespace Acme.Workforce.Absence.Tests.Balance.Assignment;

using Acme.Workforce.Absence.Balance.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BalanceDraftChecks
{
    public static void BalanceDraftStartsInDraft()
    {
        var item = BalanceDraft.NewBalanceDraft("code-298", "label-2");
        Check.Equal(item.Status, BalanceDraftStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceDraftMovesToClosed()
    {
        var item = BalanceDraft.NewBalanceDraft("code-298", "label-2");
        try
        {
            item.MoveTo(BalanceDraftStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceDraftStatus.Closed, "status");
    }

    public static void BalanceDraftRejectsMovesFromClosed()
    {
        var item = BalanceDraft.NewBalanceDraft("code-298", "label-2");
        item.Status = BalanceDraftStatus.Closed;
        Check.Fails(() => item.MoveTo(BalanceDraftStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BalanceDraftStartsInDraft();
        BalanceDraftMovesToClosed();
        BalanceDraftRejectsMovesFromClosed();
    }
}
