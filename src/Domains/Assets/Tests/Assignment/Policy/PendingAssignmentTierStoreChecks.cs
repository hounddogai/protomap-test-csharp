namespace Acme.Workforce.Assets.Tests.Assignment.Policy;

using Acme.Workforce.Assets.Assignment.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PendingAssignmentTierStoreChecks
{
    public static void MemoryPendingAssignmentTierStoreFindsWhatItSaved()
    {
        var store = PendingAssignmentTierStore.NewMemoryPendingAssignmentTierStore();
        var item = PendingAssignmentTier.NewPendingAssignmentTier("code-307", "title-719");
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
        MemoryPendingAssignmentTierStoreFindsWhatItSaved();
    }
}
