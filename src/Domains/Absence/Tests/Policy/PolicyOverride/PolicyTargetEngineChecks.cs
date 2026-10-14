namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class PolicyTargetEngineChecks
{
    public static void PolicyTargetEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyTargetEngine.DefaultPolicyTargetEngine();
        var item = PolicyTarget.NewPolicyTarget("code-635", "title-538");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyTargetEngineKeepsResultsWithinBounds();
    }
}
