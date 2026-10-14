namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class FallbackExceptionCycleEngineChecks
{
    public static void FallbackExceptionCycleEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackExceptionCycleEngine.DefaultFallbackExceptionCycleEngine();
        var item = FallbackExceptionCycle.NewFallbackExceptionCycle("code-357", "label-97");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackExceptionCycleEngineKeepsResultsWithinBounds();
    }
}
