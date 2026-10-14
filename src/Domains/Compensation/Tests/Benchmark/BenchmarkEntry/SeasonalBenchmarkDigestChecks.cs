namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class SeasonalBenchmarkDigestChecks
{
    public static void SeasonalBenchmarkDigestStartsInCreated()
    {
        var item = SeasonalBenchmarkDigest.NewSeasonalBenchmarkDigest("code-878", "label-554");
        Check.Equal(item.Status, SeasonalBenchmarkDigestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalBenchmarkDigestMovesToValidated()
    {
        var item = SeasonalBenchmarkDigest.NewSeasonalBenchmarkDigest("code-878", "label-554");
        try
        {
            item.MoveTo(SeasonalBenchmarkDigestStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalBenchmarkDigestStatus.Validated, "status");
    }

    public static void SeasonalBenchmarkDigestRejectsMovesFromRetired()
    {
        var item = SeasonalBenchmarkDigest.NewSeasonalBenchmarkDigest("code-878", "label-554");
        item.Status = SeasonalBenchmarkDigestStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalBenchmarkDigestStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalBenchmarkDigestStartsInCreated();
        SeasonalBenchmarkDigestMovesToValidated();
        SeasonalBenchmarkDigestRejectsMovesFromRetired();
    }
}
