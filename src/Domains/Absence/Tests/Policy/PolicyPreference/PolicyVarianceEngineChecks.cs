namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class PolicyVarianceEngineChecks
{
    public static void PolicyVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyVarianceEngine.DefaultPolicyVarianceEngine();
        var item = PolicyVariance.NewPolicyVariance("code-307", "label-39");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyVarianceEngineKeepsResultsWithinBounds();
    }
}
