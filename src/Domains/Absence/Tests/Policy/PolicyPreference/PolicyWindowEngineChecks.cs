namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class PolicyWindowEngineChecks
{
    public static void PolicyWindowEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyWindowEngine.DefaultPolicyWindowEngine();
        var item = PolicyWindow.NewPolicyWindow("code-704", "name-860");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyWindowEngineKeepsResultsWithinBounds();
    }
}
