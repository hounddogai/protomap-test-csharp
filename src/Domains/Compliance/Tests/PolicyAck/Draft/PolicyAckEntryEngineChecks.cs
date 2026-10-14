namespace Acme.Workforce.Compliance.Tests.PolicyAck.Draft;

using Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PolicyAckEntryEngineChecks
{
    public static void PolicyAckEntryEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckEntryEngine.DefaultPolicyAckEntryEngine();
        var item = PolicyAckEntry.NewPolicyAckEntry("code-245", "label-807");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckEntryEngineKeepsResultsWithinBounds();
    }
}
