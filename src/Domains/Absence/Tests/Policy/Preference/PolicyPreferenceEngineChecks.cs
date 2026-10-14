namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyPreferenceEngineChecks
{
    public static void PolicyPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyPreferenceEngine.DefaultPolicyPreferenceEngine(Flags.NewRegistry());
        var item = PolicyPreference.NewPolicyPreference("code-879", "title-426");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyPreferenceEngineKeepsResultsWithinBounds();
    }
}
