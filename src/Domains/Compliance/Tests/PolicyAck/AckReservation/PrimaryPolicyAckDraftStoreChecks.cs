namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckReservation;

using Acme.Workforce.Compliance.PolicyAck.AckReservation;

/// <summary>Checks of ack reservation code, run by the project's runner.</summary>
public static class PrimaryPolicyAckDraftStoreChecks
{
    public static void MemoryPrimaryPolicyAckDraftStoreFindsWhatItSaved()
    {
        var store = PrimaryPolicyAckDraftStore.NewMemoryPrimaryPolicyAckDraftStore();
        var item = PrimaryPolicyAckDraft.NewPrimaryPolicyAckDraft("code-778", "title-102");
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
        MemoryPrimaryPolicyAckDraftStoreFindsWhatItSaved();
    }
}
