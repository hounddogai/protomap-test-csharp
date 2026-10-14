namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class NominationPolicyEngineChecks
{
    public static void NominationPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = NominationPolicyEngine.DefaultNominationPolicyEngine();
        var item = NominationPolicy.NewNominationPolicy("code-334", "label-801");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationPolicyEngineKeepsResultsWithinBounds();
    }
}
