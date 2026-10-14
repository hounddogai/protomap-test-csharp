namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAudit;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of benchmark audit code, run by the project's runner.</summary>
public static class BenchmarkScheduleEngineChecks
{
    public static void BenchmarkScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkScheduleEngine.DefaultBenchmarkScheduleEngine(Flags.NewRegistry());
        var item = BenchmarkSchedule.NewBenchmarkSchedule("code-543", "label-757");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkScheduleEngineKeepsResultsWithinBounds();
    }
}
