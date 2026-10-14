namespace Acme.Workforce.Assets.Tests.Assignment.Batch;

using Acme.Workforce.Assets.Assignment.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryAssignmentTierStoreChecks
{
    public static void MemoryPrimaryAssignmentTierStoreFindsWhatItSaved()
    {
        var store = PrimaryAssignmentTierStore.NewMemoryPrimaryAssignmentTierStore();
        var item = PrimaryAssignmentTier.NewPrimaryAssignmentTier("code-75", "label-615");
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
        MemoryPrimaryAssignmentTierStoreFindsWhatItSaved();
    }
}
