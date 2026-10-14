namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GlobalPayBandAllocationStoreChecks
{
    public static void MemoryGlobalPayBandAllocationStoreFindsWhatItSaved()
    {
        var store = GlobalPayBandAllocationStore.NewMemoryGlobalPayBandAllocationStore();
        var item = GlobalPayBandAllocation.NewGlobalPayBandAllocation("code-682", "title-244");
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
        MemoryGlobalPayBandAllocationStoreFindsWhatItSaved();
    }
}
