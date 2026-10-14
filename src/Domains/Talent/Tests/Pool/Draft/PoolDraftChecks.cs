namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PoolDraftChecks
{
    public static void PoolDraftStartsInProposed()
    {
        var item = PoolDraft.NewPoolDraft("code-299", "title-374");
        Check.Equal(item.Status, PoolDraftStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolDraftMovesToRejected()
    {
        var item = PoolDraft.NewPoolDraft("code-299", "title-374");
        try
        {
            item.MoveTo(PoolDraftStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolDraftStatus.Rejected, "status");
    }

    public static void PoolDraftRejectsMovesFromWithdrawn()
    {
        var item = PoolDraft.NewPoolDraft("code-299", "title-374");
        item.Status = PoolDraftStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PoolDraftStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PoolDraftStartsInProposed();
        PoolDraftMovesToRejected();
        PoolDraftRejectsMovesFromWithdrawn();
    }
}
