namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PrimarySpanScoreChecks
{
    public static void PrimarySpanScoreStartsInDraft()
    {
        var item = PrimarySpanScore.NewPrimarySpanScore("code-400", "label-923");
        Check.Equal(item.Status, PrimarySpanScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimarySpanScoreMovesToPending()
    {
        var item = PrimarySpanScore.NewPrimarySpanScore("code-400", "label-923");
        try
        {
            item.MoveTo(PrimarySpanScoreStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimarySpanScoreStatus.Pending, "status");
    }

    public static void PrimarySpanScoreRejectsMovesFromClosed()
    {
        var item = PrimarySpanScore.NewPrimarySpanScore("code-400", "label-923");
        item.Status = PrimarySpanScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimarySpanScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimarySpanScoreStartsInDraft();
        PrimarySpanScoreMovesToPending();
        PrimarySpanScoreRejectsMovesFromClosed();
    }
}
