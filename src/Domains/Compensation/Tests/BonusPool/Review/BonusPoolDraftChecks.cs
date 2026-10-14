namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BonusPoolDraftChecks
{
    public static void BonusPoolDraftStartsInProposed()
    {
        var item = BonusPoolDraft.NewBonusPoolDraft("code-243", "label-818");
        Check.Equal(item.Status, BonusPoolDraftStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolDraftMovesToWithdrawn()
    {
        var item = BonusPoolDraft.NewBonusPoolDraft("code-243", "label-818");
        try
        {
            item.MoveTo(BonusPoolDraftStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolDraftStatus.Withdrawn, "status");
    }

    public static void BonusPoolDraftRejectsMovesFromWithdrawn()
    {
        var item = BonusPoolDraft.NewBonusPoolDraft("code-243", "label-818");
        item.Status = BonusPoolDraftStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BonusPoolDraftStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BonusPoolDraftStartsInProposed();
        BonusPoolDraftMovesToWithdrawn();
        BonusPoolDraftRejectsMovesFromWithdrawn();
    }
}
