namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckAuditEngineChecks
{
    public static void PolicyAckAuditEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckAuditEngine.DefaultPolicyAckAuditEngine();
        var item = PolicyAckAudit.NewPolicyAckAudit("code-356", "label-410");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckAuditEngineKeepsResultsWithinBounds();
    }
}
