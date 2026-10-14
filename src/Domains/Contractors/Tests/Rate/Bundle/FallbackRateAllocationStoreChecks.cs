namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackRateAllocationStoreChecks
{
    public static void MemoryFallbackRateAllocationStoreFindsWhatItSaved()
    {
        var store = FallbackRateAllocationStore.NewMemoryFallbackRateAllocationStore();
        var item = FallbackRateAllocation.NewFallbackRateAllocation("code-411", "name-285");
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
        MemoryFallbackRateAllocationStoreFindsWhatItSaved();
    }
}
