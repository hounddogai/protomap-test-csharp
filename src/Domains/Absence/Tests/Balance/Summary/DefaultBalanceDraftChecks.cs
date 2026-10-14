namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultBalanceDraftChecks
{
    public static void DefaultBalanceDraftStartsInDraft()
    {
        var item = DefaultBalanceDraft.NewDefaultBalanceDraft("code-503", "title-507");
        Check.Equal(item.Status, DefaultBalanceDraftStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultBalanceDraftMovesToActive()
    {
        var item = DefaultBalanceDraft.NewDefaultBalanceDraft("code-503", "title-507");
        try
        {
            item.MoveTo(DefaultBalanceDraftStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultBalanceDraftStatus.Active, "status");
    }

    public static void DefaultBalanceDraftRejectsMovesFromClosed()
    {
        var item = DefaultBalanceDraft.NewDefaultBalanceDraft("code-503", "title-507");
        item.Status = DefaultBalanceDraftStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultBalanceDraftStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultBalanceDraftStartsInDraft();
        DefaultBalanceDraftMovesToActive();
        DefaultBalanceDraftRejectsMovesFromClosed();
    }
}
