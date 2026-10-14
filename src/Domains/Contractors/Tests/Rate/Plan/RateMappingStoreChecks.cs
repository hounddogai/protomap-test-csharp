namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RateMappingStoreChecks
{
    public static void MemoryRateMappingStoreFindsWhatItSaved()
    {
        var store = RateMappingStore.NewMemoryRateMappingStore();
        var item = RateMapping.NewRateMapping("code-173", "title-390");
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
        MemoryRateMappingStoreFindsWhatItSaved();
    }
}
