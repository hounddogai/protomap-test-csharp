namespace Acme.Workforce.Talent.Tests.Nomination.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class NominationMappingEngineChecks
{
    public static void NominationMappingEngineKeepsResultsWithinBounds()
    {
        var engine = NominationMappingEngine.DefaultNominationMappingEngine(Flags.NewRegistry());
        var item = NominationMapping.NewNominationMapping("code-742", "name-237");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationMappingEngineKeepsResultsWithinBounds();
    }
}
