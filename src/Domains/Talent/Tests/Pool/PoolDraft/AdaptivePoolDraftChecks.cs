namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class AdaptivePoolDraftChecks
{
    public static void AdaptivePoolDraftStartsInOpen()
    {
        var item = AdaptivePoolDraft.NewAdaptivePoolDraft("code-869", "label-242");
        Check.Equal(item.Status, AdaptivePoolDraftStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePoolDraftMovesToInReview()
    {
        var item = AdaptivePoolDraft.NewAdaptivePoolDraft("code-869", "label-242");
        try
        {
            item.MoveTo(AdaptivePoolDraftStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePoolDraftStatus.InReview, "status");
    }

    public static void AdaptivePoolDraftRejectsMovesFromArchived()
    {
        var item = AdaptivePoolDraft.NewAdaptivePoolDraft("code-869", "label-242");
        item.Status = AdaptivePoolDraftStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptivePoolDraftStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptivePoolDraftStartsInOpen();
        AdaptivePoolDraftMovesToInReview();
        AdaptivePoolDraftRejectsMovesFromArchived();
    }
}
