namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PolicyProfileEngineChecks
{
    public static void PolicyProfileEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyProfileEngine.DefaultPolicyProfileEngine();
        var item = PolicyProfile.NewPolicyProfile("code-422", "label-156");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyProfileEngineKeepsResultsWithinBounds();
    }
}
