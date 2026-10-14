namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanManifestChecks
{
    public static void SpanManifestStartsInPlanned()
    {
        var item = SpanManifest.NewSpanManifest("code-151", "title-379");
        Check.Equal(item.Status, SpanManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanManifestMovesToCompleted()
    {
        var item = SpanManifest.NewSpanManifest("code-151", "title-379");
        try
        {
            item.MoveTo(SpanManifestStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanManifestStatus.Completed, "status");
    }

    public static void SpanManifestRejectsMovesFromCancelled()
    {
        var item = SpanManifest.NewSpanManifest("code-151", "title-379");
        item.Status = SpanManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SpanManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SpanManifestStartsInPlanned();
        SpanManifestMovesToCompleted();
        SpanManifestRejectsMovesFromCancelled();
    }
}
