namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAudit;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

/// <summary>Checks of benchmark audit code, run by the project's runner.</summary>
public static class StrictBenchmarkAuditEngineChecks
{
    public static void StrictBenchmarkAuditEngineKeepsResultsWithinBounds()
    {
        var engine = StrictBenchmarkAuditEngine.DefaultStrictBenchmarkAuditEngine();
        var item = StrictBenchmarkAudit.NewStrictBenchmarkAudit("code-943", "label-720");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictBenchmarkAuditEngineKeepsResultsWithinBounds();
    }
}
