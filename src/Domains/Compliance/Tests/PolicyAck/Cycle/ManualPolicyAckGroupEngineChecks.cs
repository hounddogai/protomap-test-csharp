namespace Acme.Workforce.Compliance.Tests.PolicyAck.Cycle;

using Acme.Workforce.Compliance.PolicyAck.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ManualPolicyAckGroupEngineChecks
{
    public static void ManualPolicyAckGroupEngineKeepsResultsWithinBounds()
    {
        var engine = ManualPolicyAckGroupEngine.DefaultManualPolicyAckGroupEngine();
        var item = ManualPolicyAckGroup.NewManualPolicyAckGroup("code-674", "name-237");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualPolicyAckGroupEngineKeepsResultsWithinBounds();
    }
}
