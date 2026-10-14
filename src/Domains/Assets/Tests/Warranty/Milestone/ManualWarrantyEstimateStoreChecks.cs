namespace Acme.Workforce.Assets.Tests.Warranty.Milestone;

using Acme.Workforce.Assets.Warranty.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ManualWarrantyEstimateStoreChecks
{
    public static void MemoryManualWarrantyEstimateStoreFindsWhatItSaved()
    {
        var store = ManualWarrantyEstimateStore.NewMemoryManualWarrantyEstimateStore();
        var item = ManualWarrantyEstimate.NewManualWarrantyEstimate("code-993", "label-934");
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
        MemoryManualWarrantyEstimateStoreFindsWhatItSaved();
    }
}
