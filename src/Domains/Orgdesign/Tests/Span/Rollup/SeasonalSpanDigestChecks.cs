namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalSpanDigestChecks
{
    public static void SeasonalSpanDigestStartsInIdle()
    {
        var item = SeasonalSpanDigest.NewSeasonalSpanDigest("code-5", "name-717");
        Check.Equal(item.Status, SeasonalSpanDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalSpanDigestMovesToWarming()
    {
        var item = SeasonalSpanDigest.NewSeasonalSpanDigest("code-5", "name-717");
        try
        {
            item.MoveTo(SeasonalSpanDigestStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalSpanDigestStatus.Warming, "status");
    }

    public static void SeasonalSpanDigestRejectsMovesFromStopped()
    {
        var item = SeasonalSpanDigest.NewSeasonalSpanDigest("code-5", "name-717");
        item.Status = SeasonalSpanDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalSpanDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalSpanDigestStartsInIdle();
        SeasonalSpanDigestMovesToWarming();
        SeasonalSpanDigestRejectsMovesFromStopped();
    }
}
