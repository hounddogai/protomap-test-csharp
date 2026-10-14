namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SpanGroupChecks
{
    public static void SpanGroupStartsInIdle()
    {
        var item = SpanGroup.NewSpanGroup("code-261", "label-737");
        Check.Equal(item.Status, SpanGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanGroupMovesToReady()
    {
        var item = SpanGroup.NewSpanGroup("code-261", "label-737");
        try
        {
            item.MoveTo(SpanGroupStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanGroupStatus.Ready, "status");
    }

    public static void SpanGroupRejectsMovesFromStopped()
    {
        var item = SpanGroup.NewSpanGroup("code-261", "label-737");
        item.Status = SpanGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(SpanGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SpanGroupStartsInIdle();
        SpanGroupMovesToReady();
        SpanGroupRejectsMovesFromStopped();
    }
}
