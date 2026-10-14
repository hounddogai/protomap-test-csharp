namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyRequestStoreChecks
{
    public static void MemoryWarrantyRequestStoreFindsWhatItSaved()
    {
        var store = WarrantyRequestStore.NewMemoryWarrantyRequestStore();
        var item = WarrantyRequest.NewWarrantyRequest("code-939", "title-663");
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
        MemoryWarrantyRequestStoreFindsWhatItSaved();
    }
}
