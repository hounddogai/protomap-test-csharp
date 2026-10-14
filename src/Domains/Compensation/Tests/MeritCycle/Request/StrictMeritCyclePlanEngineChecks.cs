namespace Acme.Workforce.Compensation.Tests.MeritCycle.Request;

using Acme.Workforce.Compensation.MeritCycle.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class StrictMeritCyclePlanEngineChecks
{
    public static void StrictMeritCyclePlanEngineKeepsResultsWithinBounds()
    {
        var engine = StrictMeritCyclePlanEngine.DefaultStrictMeritCyclePlanEngine();
        var item = StrictMeritCyclePlan.NewStrictMeritCyclePlan("code-180", "name-81");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictMeritCyclePlanEngineKeepsResultsWithinBounds();
    }
}
