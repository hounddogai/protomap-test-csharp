namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingBenchmarkAdjustmentEngineChecks
{
    public static void PendingBenchmarkAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = PendingBenchmarkAdjustmentEngine.DefaultPendingBenchmarkAdjustmentEngine();
        var item = PendingBenchmarkAdjustment.NewPendingBenchmarkAdjustment("code-760", "title-586");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingBenchmarkAdjustmentEngineKeepsResultsWithinBounds();
    }
}
