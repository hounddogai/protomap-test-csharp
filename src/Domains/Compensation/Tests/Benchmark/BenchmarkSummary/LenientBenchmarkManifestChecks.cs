namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class LenientBenchmarkManifestChecks
{
    public static void LenientBenchmarkManifestStartsInIdle()
    {
        var item = LenientBenchmarkManifest.NewLenientBenchmarkManifest("code-383", "name-521");
        Check.Equal(item.Status, LenientBenchmarkManifestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientBenchmarkManifestMovesToStopped()
    {
        var item = LenientBenchmarkManifest.NewLenientBenchmarkManifest("code-383", "name-521");
        try
        {
            item.MoveTo(LenientBenchmarkManifestStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientBenchmarkManifestStatus.Stopped, "status");
    }

    public static void LenientBenchmarkManifestRejectsMovesFromStopped()
    {
        var item = LenientBenchmarkManifest.NewLenientBenchmarkManifest("code-383", "name-521");
        item.Status = LenientBenchmarkManifestStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientBenchmarkManifestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientBenchmarkManifestStartsInIdle();
        LenientBenchmarkManifestMovesToStopped();
        LenientBenchmarkManifestRejectsMovesFromStopped();
    }
}
