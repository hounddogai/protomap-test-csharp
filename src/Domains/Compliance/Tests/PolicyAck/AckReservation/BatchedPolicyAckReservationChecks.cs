namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckReservation;

using Acme.Workforce.Compliance.PolicyAck.AckReservation;

/// <summary>Checks of ack reservation code, run by the project's runner.</summary>
public static class BatchedPolicyAckReservationChecks
{
    public static void BatchedPolicyAckReservationStartsInCreated()
    {
        var item = BatchedPolicyAckReservation.NewBatchedPolicyAckReservation("code-956", "label-220");
        Check.Equal(item.Status, BatchedPolicyAckReservationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPolicyAckReservationMovesToRetired()
    {
        var item = BatchedPolicyAckReservation.NewBatchedPolicyAckReservation("code-956", "label-220");
        try
        {
            item.MoveTo(BatchedPolicyAckReservationStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPolicyAckReservationStatus.Retired, "status");
    }

    public static void BatchedPolicyAckReservationRejectsMovesFromRetired()
    {
        var item = BatchedPolicyAckReservation.NewBatchedPolicyAckReservation("code-956", "label-220");
        item.Status = BatchedPolicyAckReservationStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedPolicyAckReservationStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedPolicyAckReservationStartsInCreated();
        BatchedPolicyAckReservationMovesToRetired();
        BatchedPolicyAckReservationRejectsMovesFromRetired();
    }
}
