namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TieredBenchmarkScoreEngineChecks
{
    public static void TieredBenchmarkScoreEngineKeepsResultsWithinBounds()
    {
        var engine = TieredBenchmarkScoreEngine.DefaultTieredBenchmarkScoreEngine();
        var item = TieredBenchmarkScore.NewTieredBenchmarkScore("code-598", "name-864");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredBenchmarkScoreEngineKeepsResultsWithinBounds();
    }
}
