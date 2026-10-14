namespace Acme.Workforce.Assets.Tests.Assignment.Exception;

using Acme.Workforce.Assets.Assignment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class RegionalAssignmentTierStoreChecks
{
    public static void MemoryRegionalAssignmentTierStoreFindsWhatItSaved()
    {
        var store = RegionalAssignmentTierStore.NewMemoryRegionalAssignmentTierStore();
        var item = RegionalAssignmentTier.NewRegionalAssignmentTier("code-348", "name-327");
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
        MemoryRegionalAssignmentTierStoreFindsWhatItSaved();
    }
}
