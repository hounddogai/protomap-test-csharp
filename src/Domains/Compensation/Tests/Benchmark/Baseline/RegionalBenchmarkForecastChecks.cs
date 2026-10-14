namespace Acme.Workforce.Compensation.Tests.Benchmark.Baseline;

using Acme.Workforce.Compensation.Benchmark.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RegionalBenchmarkForecastChecks
{
    public static void RegionalBenchmarkForecastStartsInIdle()
    {
        var item = RegionalBenchmarkForecast.NewRegionalBenchmarkForecast("code-464", "name-7");
        Check.Equal(item.Status, RegionalBenchmarkForecastStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalBenchmarkForecastMovesToDraining()
    {
        var item = RegionalBenchmarkForecast.NewRegionalBenchmarkForecast("code-464", "name-7");
        try
        {
            item.MoveTo(RegionalBenchmarkForecastStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalBenchmarkForecastStatus.Draining, "status");
    }

    public static void RegionalBenchmarkForecastRejectsMovesFromStopped()
    {
        var item = RegionalBenchmarkForecast.NewRegionalBenchmarkForecast("code-464", "name-7");
        item.Status = RegionalBenchmarkForecastStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalBenchmarkForecastStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalBenchmarkForecastStartsInIdle();
        RegionalBenchmarkForecastMovesToDraining();
        RegionalBenchmarkForecastRejectsMovesFromStopped();
    }
}
