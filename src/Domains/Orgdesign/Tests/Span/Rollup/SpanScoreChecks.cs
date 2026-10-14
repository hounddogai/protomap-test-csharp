namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanScoreChecks
{
    public static void SpanScoreStartsInPlanned()
    {
        var item = SpanScore.NewSpanScore("code-175", "title-609");
        Check.Equal(item.Status, SpanScoreStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanScoreMovesToInProgress()
    {
        var item = SpanScore.NewSpanScore("code-175", "title-609");
        try
        {
            item.MoveTo(SpanScoreStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanScoreStatus.InProgress, "status");
    }

    public static void SpanScoreRejectsMovesFromCancelled()
    {
        var item = SpanScore.NewSpanScore("code-175", "title-609");
        item.Status = SpanScoreStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SpanScoreStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SpanScoreStartsInPlanned();
        SpanScoreMovesToInProgress();
        SpanScoreRejectsMovesFromCancelled();
    }
}
