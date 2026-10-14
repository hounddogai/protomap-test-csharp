namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalBenchmarkReviewEngineChecks
{
    public static void RegionalBenchmarkReviewEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalBenchmarkReviewEngine.DefaultRegionalBenchmarkReviewEngine(Flags.NewRegistry());
        var item = RegionalBenchmarkReview.NewRegionalBenchmarkReview("code-783", "name-846");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalBenchmarkReviewEngineKeepsResultsWithinBounds();
    }
}
