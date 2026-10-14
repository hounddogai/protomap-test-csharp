namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LenientWarrantyForecastStoreChecks
{
    public static void MemoryLenientWarrantyForecastStoreFindsWhatItSaved()
    {
        var store = LenientWarrantyForecastStore.NewMemoryLenientWarrantyForecastStore();
        var item = LenientWarrantyForecast.NewLenientWarrantyForecast("code-675", "label-486");
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
        MemoryLenientWarrantyForecastStoreFindsWhatItSaved();
    }
}
