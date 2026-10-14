namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class RegionalPoolMetricChecks
{
    public static void RegionalPoolMetricStartsInIdle()
    {
        var item = RegionalPoolMetric.NewRegionalPoolMetric("code-188", "name-843");
        Check.Equal(item.Status, RegionalPoolMetricStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalPoolMetricMovesToReady()
    {
        var item = RegionalPoolMetric.NewRegionalPoolMetric("code-188", "name-843");
        try
        {
            item.MoveTo(RegionalPoolMetricStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalPoolMetricStatus.Ready, "status");
    }

    public static void RegionalPoolMetricRejectsMovesFromStopped()
    {
        var item = RegionalPoolMetric.NewRegionalPoolMetric("code-188", "name-843");
        item.Status = RegionalPoolMetricStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalPoolMetricStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalPoolMetricStartsInIdle();
        RegionalPoolMetricMovesToReady();
        RegionalPoolMetricRejectsMovesFromStopped();
    }
}
