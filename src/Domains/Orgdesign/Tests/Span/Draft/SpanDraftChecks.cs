namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SpanDraftChecks
{
    public static void SpanDraftStartsInDraft()
    {
        var item = SpanDraft.NewSpanDraft("code-479", "title-818");
        Check.Equal(item.Status, SpanDraftStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanDraftMovesToPending()
    {
        var item = SpanDraft.NewSpanDraft("code-479", "title-818");
        try
        {
            item.MoveTo(SpanDraftStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanDraftStatus.Pending, "status");
    }

    public static void SpanDraftRejectsMovesFromClosed()
    {
        var item = SpanDraft.NewSpanDraft("code-479", "title-818");
        item.Status = SpanDraftStatus.Closed;
        Check.Fails(() => item.MoveTo(SpanDraftStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SpanDraftStartsInDraft();
        SpanDraftMovesToPending();
        SpanDraftRejectsMovesFromClosed();
    }
}
