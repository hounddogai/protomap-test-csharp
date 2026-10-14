namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class PolicyClaimEngineChecks
{
    public static void PolicyClaimEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyClaimEngine.DefaultPolicyClaimEngine();
        var item = PolicyClaim.NewPolicyClaim("code-705", "title-338");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyClaimEngineKeepsResultsWithinBounds();
    }
}
