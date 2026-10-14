namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class WeightedSpanBaselineChecks
{
    public static void WeightedSpanBaselineStartsInIdle()
    {
        var item = WeightedSpanBaseline.NewWeightedSpanBaseline("code-958", "title-987");
        Check.Equal(item.Status, WeightedSpanBaselineStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedSpanBaselineMovesToReady()
    {
        var item = WeightedSpanBaseline.NewWeightedSpanBaseline("code-958", "title-987");
        try
        {
            item.MoveTo(WeightedSpanBaselineStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedSpanBaselineStatus.Ready, "status");
    }

    public static void WeightedSpanBaselineRejectsMovesFromStopped()
    {
        var item = WeightedSpanBaseline.NewWeightedSpanBaseline("code-958", "title-987");
        item.Status = WeightedSpanBaselineStatus.Stopped;
        Check.Fails(() => item.MoveTo(WeightedSpanBaselineStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WeightedSpanBaselineStartsInIdle();
        WeightedSpanBaselineMovesToReady();
        WeightedSpanBaselineRejectsMovesFromStopped();
    }
}
