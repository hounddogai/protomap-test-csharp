namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class RegionalNominationPolicyEngineChecks
{
    public static void RegionalNominationPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalNominationPolicyEngine.DefaultRegionalNominationPolicyEngine(Flags.NewRegistry());
        var item = RegionalNominationPolicy.NewRegionalNominationPolicy("code-835", "name-358");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalNominationPolicyEngineKeepsResultsWithinBounds();
    }
}
