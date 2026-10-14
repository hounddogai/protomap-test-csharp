namespace Acme.Workforce.Orgdesign.Tests.Span.Digest;

using Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SpanDigestChecks
{
    public static void SpanDigestStartsInRequested()
    {
        var item = SpanDigest.NewSpanDigest("code-675", "name-571");
        Check.Equal(item.Status, SpanDigestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanDigestMovesToGranted()
    {
        var item = SpanDigest.NewSpanDigest("code-675", "name-571");
        try
        {
            item.MoveTo(SpanDigestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanDigestStatus.Granted, "status");
    }

    public static void SpanDigestRejectsMovesFromExpired()
    {
        var item = SpanDigest.NewSpanDigest("code-675", "name-571");
        item.Status = SpanDigestStatus.Expired;
        Check.Fails(() => item.MoveTo(SpanDigestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SpanDigestStartsInRequested();
        SpanDigestMovesToGranted();
        SpanDigestRejectsMovesFromExpired();
    }
}
