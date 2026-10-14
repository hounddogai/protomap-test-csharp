namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FallbackPayBandMappingStoreChecks
{
    public static void MemoryFallbackPayBandMappingStoreFindsWhatItSaved()
    {
        var store = FallbackPayBandMappingStore.NewMemoryFallbackPayBandMappingStore();
        var item = FallbackPayBandMapping.NewFallbackPayBandMapping("code-834", "title-211");
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
        MemoryFallbackPayBandMappingStoreFindsWhatItSaved();
    }
}
