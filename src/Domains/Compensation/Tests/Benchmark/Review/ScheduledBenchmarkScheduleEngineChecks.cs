namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledBenchmarkScheduleEngineChecks
{
    public static void ScheduledBenchmarkScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledBenchmarkScheduleEngine.DefaultScheduledBenchmarkScheduleEngine();
        var item = ScheduledBenchmarkSchedule.NewScheduledBenchmarkSchedule("code-16", "label-237");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledBenchmarkScheduleEngineKeepsResultsWithinBounds();
    }
}
