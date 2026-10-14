namespace Acme.Workforce.Compliance.Tests.PolicyAck.Milestone;

using Acme.Workforce.Compliance.PolicyAck.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LenientPolicyAckMilestoneStoreChecks
{
    public static void MemoryLenientPolicyAckMilestoneStoreFindsWhatItSaved()
    {
        var store = LenientPolicyAckMilestoneStore.NewMemoryLenientPolicyAckMilestoneStore();
        var item = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone("code-118", "label-282");
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
        MemoryLenientPolicyAckMilestoneStoreFindsWhatItSaved();
    }
}
