namespace Acme.Workforce.Orgdesign.Tests.Span.SpanDraft;

using Acme.Workforce.Orgdesign.Span.SpanDraft;

/// <summary>Checks of span draft code, run by the project's runner.</summary>
public static class GlobalSpanDraftChecks
{
    public static void GlobalSpanDraftStartsInNew()
    {
        var item = GlobalSpanDraft.NewGlobalSpanDraft("code-907", "title-833");
        Check.Equal(item.Status, GlobalSpanDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalSpanDraftMovesToRunning()
    {
        var item = GlobalSpanDraft.NewGlobalSpanDraft("code-907", "title-833");
        try
        {
            item.MoveTo(GlobalSpanDraftStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalSpanDraftStatus.Running, "status");
    }

    public static void GlobalSpanDraftRejectsMovesFromFailed()
    {
        var item = GlobalSpanDraft.NewGlobalSpanDraft("code-907", "title-833");
        item.Status = GlobalSpanDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(GlobalSpanDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        GlobalSpanDraftStartsInNew();
        GlobalSpanDraftMovesToRunning();
        GlobalSpanDraftRejectsMovesFromFailed();
    }
}
