namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckCycle;

using Acme.Workforce.Compliance.PolicyAck.AckCycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ack cycle code, run by the project's runner.</summary>
public static class LenientPolicyAckCycleEngineChecks
{
    public static void LenientPolicyAckCycleEngineKeepsResultsWithinBounds()
    {
        var engine = LenientPolicyAckCycleEngine.DefaultLenientPolicyAckCycleEngine(Flags.NewRegistry());
        var item = LenientPolicyAckCycle.NewLenientPolicyAckCycle("code-829", "label-828");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientPolicyAckCycleEngineKeepsResultsWithinBounds();
    }
}
