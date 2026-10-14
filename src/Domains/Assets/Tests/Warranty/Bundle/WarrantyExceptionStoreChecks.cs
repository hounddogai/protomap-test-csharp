namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WarrantyExceptionStoreChecks
{
    public static void MemoryWarrantyExceptionStoreFindsWhatItSaved()
    {
        var store = WarrantyExceptionStore.NewMemoryWarrantyExceptionStore();
        var item = WarrantyException.NewWarrantyException("code-534", "label-643");
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
        MemoryWarrantyExceptionStoreFindsWhatItSaved();
    }
}
