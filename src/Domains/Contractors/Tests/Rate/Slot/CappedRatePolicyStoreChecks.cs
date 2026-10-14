namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CappedRatePolicyStoreChecks
{
    public static void MemoryCappedRatePolicyStoreFindsWhatItSaved()
    {
        var store = CappedRatePolicyStore.NewMemoryCappedRatePolicyStore();
        var item = CappedRatePolicy.NewCappedRatePolicy("code-791", "title-783");
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
        MemoryCappedRatePolicyStoreFindsWhatItSaved();
    }
}
