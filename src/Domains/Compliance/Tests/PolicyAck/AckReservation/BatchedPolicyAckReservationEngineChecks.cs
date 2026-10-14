namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckReservation;

using Acme.Workforce.Compliance.PolicyAck.AckReservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ack reservation code, run by the project's runner.</summary>
public static class BatchedPolicyAckReservationEngineChecks
{
    public static void BatchedPolicyAckReservationEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedPolicyAckReservationEngine.DefaultBatchedPolicyAckReservationEngine(Flags.NewRegistry());
        var item = BatchedPolicyAckReservation.NewBatchedPolicyAckReservation("code-956", "label-220");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedPolicyAckReservationEngineKeepsResultsWithinBounds();
    }
}
