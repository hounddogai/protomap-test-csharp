namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class DefaultSpanBaselineChecks
{
    public static void DefaultSpanBaselineStartsInIdle()
    {
        var item = DefaultSpanBaseline.NewDefaultSpanBaseline("code-946", "name-22");
        Check.Equal(item.Status, DefaultSpanBaselineStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultSpanBaselineMovesToDraining()
    {
        var item = DefaultSpanBaseline.NewDefaultSpanBaseline("code-946", "name-22");
        try
        {
            item.MoveTo(DefaultSpanBaselineStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultSpanBaselineStatus.Draining, "status");
    }

    public static void DefaultSpanBaselineRejectsMovesFromStopped()
    {
        var item = DefaultSpanBaseline.NewDefaultSpanBaseline("code-946", "name-22");
        item.Status = DefaultSpanBaselineStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultSpanBaselineStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultSpanBaselineStartsInIdle();
        DefaultSpanBaselineMovesToDraining();
        DefaultSpanBaselineRejectsMovesFromStopped();
    }
}
