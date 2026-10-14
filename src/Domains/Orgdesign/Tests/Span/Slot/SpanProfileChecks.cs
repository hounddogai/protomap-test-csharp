namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanProfileChecks
{
    public static void SpanProfileStartsInIdle()
    {
        var item = SpanProfile.NewSpanProfile("code-753", "label-297");
        Check.Equal(item.Status, SpanProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanProfileMovesToWarming()
    {
        var item = SpanProfile.NewSpanProfile("code-753", "label-297");
        try
        {
            item.MoveTo(SpanProfileStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanProfileStatus.Warming, "status");
    }

    public static void SpanProfileRejectsMovesFromStopped()
    {
        var item = SpanProfile.NewSpanProfile("code-753", "label-297");
        item.Status = SpanProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(SpanProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SpanProfileStartsInIdle();
        SpanProfileMovesToWarming();
        SpanProfileRejectsMovesFromStopped();
    }
}
