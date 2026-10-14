namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RollingBenchmarkProfileEngineChecks
{
    public static void RollingBenchmarkProfileEngineKeepsResultsWithinBounds()
    {
        var engine = RollingBenchmarkProfileEngine.DefaultRollingBenchmarkProfileEngine(Flags.NewRegistry());
        var item = RollingBenchmarkProfile.NewRollingBenchmarkProfile("code-486", "title-65");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingBenchmarkProfileEngineKeepsResultsWithinBounds();
    }
}
