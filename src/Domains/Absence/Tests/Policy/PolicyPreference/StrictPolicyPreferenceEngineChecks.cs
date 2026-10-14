namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class StrictPolicyPreferenceEngineChecks
{
    public static void StrictPolicyPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPolicyPreferenceEngine.DefaultStrictPolicyPreferenceEngine();
        var item = StrictPolicyPreference.NewStrictPolicyPreference("code-691", "title-468");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPolicyPreferenceEngineKeepsResultsWithinBounds();
    }
}
