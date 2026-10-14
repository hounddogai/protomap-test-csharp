namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class StrictPoolScheduleEngineChecks
{
    public static void StrictPoolScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPoolScheduleEngine.DefaultStrictPoolScheduleEngine();
        var item = StrictPoolSchedule.NewStrictPoolSchedule("code-70", "name-626");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPoolScheduleEngineKeepsResultsWithinBounds();
    }
}
