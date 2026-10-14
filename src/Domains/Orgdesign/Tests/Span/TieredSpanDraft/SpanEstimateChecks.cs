namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanEstimateChecks
{
    public static void SpanEstimateStartsInIdle()
    {
        var item = SpanEstimate.NewSpanEstimate("code-978", "name-708");
        Check.Equal(item.Status, SpanEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanEstimateMovesToDraining()
    {
        var item = SpanEstimate.NewSpanEstimate("code-978", "name-708");
        try
        {
            item.MoveTo(SpanEstimateStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanEstimateStatus.Draining, "status");
    }

    public static void SpanEstimateRejectsMovesFromStopped()
    {
        var item = SpanEstimate.NewSpanEstimate("code-978", "name-708");
        item.Status = SpanEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(SpanEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SpanEstimateStartsInIdle();
        SpanEstimateMovesToDraining();
        SpanEstimateRejectsMovesFromStopped();
    }
}
