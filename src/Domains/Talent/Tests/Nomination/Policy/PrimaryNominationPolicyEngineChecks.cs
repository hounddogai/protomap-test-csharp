namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PrimaryNominationPolicyEngineChecks
{
    public static void PrimaryNominationPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryNominationPolicyEngine.DefaultPrimaryNominationPolicyEngine();
        var item = PrimaryNominationPolicy.NewPrimaryNominationPolicy("code-319", "title-30");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryNominationPolicyEngineKeepsResultsWithinBounds();
    }
}
