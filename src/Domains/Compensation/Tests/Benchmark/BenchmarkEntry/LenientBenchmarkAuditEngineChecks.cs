namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class LenientBenchmarkAuditEngineChecks
{
    public static void LenientBenchmarkAuditEngineKeepsResultsWithinBounds()
    {
        var engine = LenientBenchmarkAuditEngine.DefaultLenientBenchmarkAuditEngine(Flags.NewRegistry());
        var item = LenientBenchmarkAudit.NewLenientBenchmarkAudit("code-716", "title-308");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientBenchmarkAuditEngineKeepsResultsWithinBounds();
    }
}
