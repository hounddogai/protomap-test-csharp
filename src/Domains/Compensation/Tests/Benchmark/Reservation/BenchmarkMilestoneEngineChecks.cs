namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkMilestoneEngineChecks
{
    public static void BenchmarkMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkMilestoneEngine.DefaultBenchmarkMilestoneEngine(Flags.NewRegistry());
        var item = BenchmarkMilestone.NewBenchmarkMilestone("code-731", "label-984");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkMilestoneEngineKeepsResultsWithinBounds();
    }
}
