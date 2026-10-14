namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class SpanThresholdChecks
{
    public static void SpanThresholdStartsInProposed()
    {
        var item = SpanThreshold.NewSpanThreshold("code-974", "name-532");
        Check.Equal(item.Status, SpanThresholdStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanThresholdMovesToRejected()
    {
        var item = SpanThreshold.NewSpanThreshold("code-974", "name-532");
        try
        {
            item.MoveTo(SpanThresholdStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanThresholdStatus.Rejected, "status");
    }

    public static void SpanThresholdRejectsMovesFromWithdrawn()
    {
        var item = SpanThreshold.NewSpanThreshold("code-974", "name-532");
        item.Status = SpanThresholdStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SpanThresholdStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SpanThresholdStartsInProposed();
        SpanThresholdMovesToRejected();
        SpanThresholdRejectsMovesFromWithdrawn();
    }
}
