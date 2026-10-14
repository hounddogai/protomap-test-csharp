namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalPolicyVarianceEngineChecks
{
    public static void RegionalPolicyVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalPolicyVarianceEngine.DefaultRegionalPolicyVarianceEngine();
        var item = RegionalPolicyVariance.NewRegionalPolicyVariance("code-735", "name-224");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalPolicyVarianceEngineKeepsResultsWithinBounds();
    }
}
