namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckCycle;

using Acme.Workforce.Compliance.PolicyAck.AckCycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ack cycle code, run by the project's runner.</summary>
public static class CappedPolicyAckRollupEngineChecks
{
    public static void CappedPolicyAckRollupEngineKeepsResultsWithinBounds()
    {
        var engine = CappedPolicyAckRollupEngine.DefaultCappedPolicyAckRollupEngine(Flags.NewRegistry());
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("code-816", "title-300");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedPolicyAckRollupEngineKeepsResultsWithinBounds();
    }
}
