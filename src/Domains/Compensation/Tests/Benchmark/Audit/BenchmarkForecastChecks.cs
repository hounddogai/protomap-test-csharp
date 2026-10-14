namespace Acme.Workforce.Compensation.Tests.Benchmark.Audit;

using Acme.Workforce.Compensation.Benchmark.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BenchmarkForecastChecks
{
    public static void BenchmarkForecastStartsInProposed()
    {
        var item = BenchmarkForecast.NewBenchmarkForecast("code-903", "label-470");
        Check.Equal(item.Status, BenchmarkForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkForecastMovesToApproved()
    {
        var item = BenchmarkForecast.NewBenchmarkForecast("code-903", "label-470");
        try
        {
            item.MoveTo(BenchmarkForecastStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkForecastStatus.Approved, "status");
    }

    public static void BenchmarkForecastRejectsMovesFromWithdrawn()
    {
        var item = BenchmarkForecast.NewBenchmarkForecast("code-903", "label-470");
        item.Status = BenchmarkForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BenchmarkForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BenchmarkForecastStartsInProposed();
        BenchmarkForecastMovesToApproved();
        BenchmarkForecastRejectsMovesFromWithdrawn();
    }
}
