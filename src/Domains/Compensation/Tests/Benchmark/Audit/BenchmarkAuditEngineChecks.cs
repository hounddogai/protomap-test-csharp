namespace Acme.Workforce.Compensation.Tests.Benchmark.Audit;

using Acme.Workforce.Compensation.Benchmark.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BenchmarkAuditEngineChecks
{
    public static void BenchmarkAuditEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkAuditEngine.DefaultBenchmarkAuditEngine(Flags.NewRegistry());
        var item = BenchmarkAudit.NewBenchmarkAudit("code-974", "name-359");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkAuditEngineKeepsResultsWithinBounds();
    }
}
