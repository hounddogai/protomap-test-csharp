namespace Acme.Workforce.Contractors.Tests.Rate.Mapping;

using Acme.Workforce.Contractors.Rate.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class StrictRateMappingEngineChecks
{
    public static void StrictRateMappingEngineKeepsResultsWithinBounds()
    {
        var engine = StrictRateMappingEngine.DefaultStrictRateMappingEngine(Flags.NewRegistry());
        var item = StrictRateMapping.NewStrictRateMapping("code-559", "name-625");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictRateMappingEngineKeepsResultsWithinBounds();
    }
}
