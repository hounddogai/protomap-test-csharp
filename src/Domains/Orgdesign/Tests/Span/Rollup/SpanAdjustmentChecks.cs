namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanAdjustmentChecks
{
    public static void SpanAdjustmentStartsInIdle()
    {
        var item = SpanAdjustment.NewSpanAdjustment("code-315", "name-222");
        Check.Equal(item.Status, SpanAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanAdjustmentMovesToReady()
    {
        var item = SpanAdjustment.NewSpanAdjustment("code-315", "name-222");
        try
        {
            item.MoveTo(SpanAdjustmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanAdjustmentStatus.Ready, "status");
    }

    public static void SpanAdjustmentRejectsMovesFromStopped()
    {
        var item = SpanAdjustment.NewSpanAdjustment("code-315", "name-222");
        item.Status = SpanAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(SpanAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SpanAdjustmentStartsInIdle();
        SpanAdjustmentMovesToReady();
        SpanAdjustmentRejectsMovesFromStopped();
    }
}
