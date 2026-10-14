namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientPoolDraftChecks
{
    public static void LenientPoolDraftStartsInNew()
    {
        var item = LenientPoolDraft.NewLenientPoolDraft("code-535", "title-477");
        Check.Equal(item.Status, LenientPoolDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPoolDraftMovesToFailed()
    {
        var item = LenientPoolDraft.NewLenientPoolDraft("code-535", "title-477");
        try
        {
            item.MoveTo(LenientPoolDraftStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPoolDraftStatus.Failed, "status");
    }

    public static void LenientPoolDraftRejectsMovesFromFailed()
    {
        var item = LenientPoolDraft.NewLenientPoolDraft("code-535", "title-477");
        item.Status = LenientPoolDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientPoolDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientPoolDraftStartsInNew();
        LenientPoolDraftMovesToFailed();
        LenientPoolDraftRejectsMovesFromFailed();
    }
}
