namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkReviewEngineChecks
{
    public static void BenchmarkReviewEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkReviewEngine.DefaultBenchmarkReviewEngine(Flags.NewRegistry());
        var item = BenchmarkReview.NewBenchmarkReview("code-397", "label-744");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkReviewEngineKeepsResultsWithinBounds();
    }
}
