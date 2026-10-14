namespace Acme.Workforce.Talent.Tests.Pool.Variance;

using Acme.Workforce.Talent.Pool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GlobalPoolCycleEngineChecks
{
    public static void GlobalPoolCycleEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPoolCycleEngine.DefaultGlobalPoolCycleEngine();
        var item = GlobalPoolCycle.NewGlobalPoolCycle("code-549", "name-411");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPoolCycleEngineKeepsResultsWithinBounds();
    }
}
