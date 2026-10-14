namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanRollupChecks
{
    public static void SpanRollupStartsInOpen()
    {
        var item = SpanRollup.NewSpanRollup("code-807", "name-805");
        Check.Equal(item.Status, SpanRollupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanRollupMovesToInReview()
    {
        var item = SpanRollup.NewSpanRollup("code-807", "name-805");
        try
        {
            item.MoveTo(SpanRollupStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanRollupStatus.InReview, "status");
    }

    public static void SpanRollupRejectsMovesFromArchived()
    {
        var item = SpanRollup.NewSpanRollup("code-807", "name-805");
        item.Status = SpanRollupStatus.Archived;
        Check.Fails(() => item.MoveTo(SpanRollupStatus.Open), "move");
    }

    public static void RunAll()
    {
        SpanRollupStartsInOpen();
        SpanRollupMovesToInReview();
        SpanRollupRejectsMovesFromArchived();
    }
}
