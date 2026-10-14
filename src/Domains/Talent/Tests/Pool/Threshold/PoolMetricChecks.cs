namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolMetricChecks
{
    public static void PoolMetricStartsInIdle()
    {
        var item = PoolMetric.NewPoolMetric("code-271", "title-731");
        Check.Equal(item.Status, PoolMetricStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolMetricMovesToWarming()
    {
        var item = PoolMetric.NewPoolMetric("code-271", "title-731");
        try
        {
            item.MoveTo(PoolMetricStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolMetricStatus.Warming, "status");
    }

    public static void PoolMetricRejectsMovesFromStopped()
    {
        var item = PoolMetric.NewPoolMetric("code-271", "title-731");
        item.Status = PoolMetricStatus.Stopped;
        Check.Fails(() => item.MoveTo(PoolMetricStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PoolMetricStartsInIdle();
        PoolMetricMovesToWarming();
        PoolMetricRejectsMovesFromStopped();
    }
}
