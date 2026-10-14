namespace Acme.Workforce.Orgdesign.Tests.Reorg.Draft;

using Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictReorgDraftChecks
{
    public static void StrictReorgDraftStartsInProposed()
    {
        var item = StrictReorgDraft.NewStrictReorgDraft("code-736", "title-703");
        Check.Equal(item.Status, StrictReorgDraftStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictReorgDraftMovesToApproved()
    {
        var item = StrictReorgDraft.NewStrictReorgDraft("code-736", "title-703");
        try
        {
            item.MoveTo(StrictReorgDraftStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictReorgDraftStatus.Approved, "status");
    }

    public static void StrictReorgDraftRejectsMovesFromWithdrawn()
    {
        var item = StrictReorgDraft.NewStrictReorgDraft("code-736", "title-703");
        item.Status = StrictReorgDraftStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictReorgDraftStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictReorgDraftStartsInProposed();
        StrictReorgDraftMovesToApproved();
        StrictReorgDraftRejectsMovesFromWithdrawn();
    }
}
