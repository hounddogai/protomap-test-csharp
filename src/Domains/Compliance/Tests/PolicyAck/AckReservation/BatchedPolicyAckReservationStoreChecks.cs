namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckReservation;

using Acme.Workforce.Compliance.PolicyAck.AckReservation;

/// <summary>Checks of ack reservation code, run by the project's runner.</summary>
public static class BatchedPolicyAckReservationStoreChecks
{
    public static void MemoryBatchedPolicyAckReservationStoreFindsWhatItSaved()
    {
        var store = BatchedPolicyAckReservationStore.NewMemoryBatchedPolicyAckReservationStore();
        var item = BatchedPolicyAckReservation.NewBatchedPolicyAckReservation("code-956", "label-220");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryBatchedPolicyAckReservationStoreFindsWhatItSaved();
    }
}
