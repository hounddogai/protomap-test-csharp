namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class SeasonalExceptionGroupEngineChecks
{
    public static void SeasonalExceptionGroupEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalExceptionGroupEngine.DefaultSeasonalExceptionGroupEngine(Flags.NewRegistry());
        var item = SeasonalExceptionGroup.NewSeasonalExceptionGroup("code-16", "label-475");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalExceptionGroupEngineKeepsResultsWithinBounds();
    }
}
