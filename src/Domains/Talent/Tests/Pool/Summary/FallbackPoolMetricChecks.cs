namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FallbackPoolMetricChecks
{
    public static void FallbackPoolMetricStartsInPlanned()
    {
        var item = FallbackPoolMetric.NewFallbackPoolMetric("code-776", "name-246");
        Check.Equal(item.Status, FallbackPoolMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPoolMetricMovesToInProgress()
    {
        var item = FallbackPoolMetric.NewFallbackPoolMetric("code-776", "name-246");
        try
        {
            item.MoveTo(FallbackPoolMetricStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPoolMetricStatus.InProgress, "status");
    }

    public static void FallbackPoolMetricRejectsMovesFromCancelled()
    {
        var item = FallbackPoolMetric.NewFallbackPoolMetric("code-776", "name-246");
        item.Status = FallbackPoolMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackPoolMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackPoolMetricStartsInPlanned();
        FallbackPoolMetricMovesToInProgress();
        FallbackPoolMetricRejectsMovesFromCancelled();
    }
}
