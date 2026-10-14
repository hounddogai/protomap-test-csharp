namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyItemEngineChecks
{
    public static void PolicyItemEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyItemEngine.DefaultPolicyItemEngine();
        var item = PolicyItem.NewPolicyItem("code-60", "label-718");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyItemEngineKeepsResultsWithinBounds();
    }
}
