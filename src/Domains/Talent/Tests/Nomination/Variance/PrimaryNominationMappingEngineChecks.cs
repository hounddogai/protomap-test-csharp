namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PrimaryNominationMappingEngineChecks
{
    public static void PrimaryNominationMappingEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryNominationMappingEngine.DefaultPrimaryNominationMappingEngine(Flags.NewRegistry());
        var item = PrimaryNominationMapping.NewPrimaryNominationMapping("code-736", "name-262");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryNominationMappingEngineKeepsResultsWithinBounds();
    }
}
