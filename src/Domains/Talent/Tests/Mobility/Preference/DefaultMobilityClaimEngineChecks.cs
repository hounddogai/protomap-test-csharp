namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultMobilityClaimEngineChecks
{
    public static void DefaultMobilityClaimEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultMobilityClaimEngine.DefaultDefaultMobilityClaimEngine(Flags.NewRegistry());
        var item = DefaultMobilityClaim.NewDefaultMobilityClaim("code-628", "name-29");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultMobilityClaimEngineKeepsResultsWithinBounds();
    }
}
