namespace Acme.Workforce.Compensation.Tests.Benchmark.Entry;

using Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BenchmarkManifestChecks
{
    public static void BenchmarkManifestStartsInCreated()
    {
        var item = BenchmarkManifest.NewBenchmarkManifest("code-302", "label-773");
        Check.Equal(item.Status, BenchmarkManifestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkManifestMovesToRetired()
    {
        var item = BenchmarkManifest.NewBenchmarkManifest("code-302", "label-773");
        try
        {
            item.MoveTo(BenchmarkManifestStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkManifestStatus.Retired, "status");
    }

    public static void BenchmarkManifestRejectsMovesFromRetired()
    {
        var item = BenchmarkManifest.NewBenchmarkManifest("code-302", "label-773");
        item.Status = BenchmarkManifestStatus.Retired;
        Check.Fails(() => item.MoveTo(BenchmarkManifestStatus.Created), "move");
    }

    public static void RunAll()
    {
        BenchmarkManifestStartsInCreated();
        BenchmarkManifestMovesToRetired();
        BenchmarkManifestRejectsMovesFromRetired();
    }
}
