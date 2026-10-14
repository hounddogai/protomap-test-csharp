namespace Acme.Workforce.Assets.Tests.Warranty.Template;

using Acme.Workforce.Assets.Warranty.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WarrantyTemplateStoreChecks
{
    public static void MemoryWarrantyTemplateStoreFindsWhatItSaved()
    {
        var store = WarrantyTemplateStore.NewMemoryWarrantyTemplateStore();
        var item = WarrantyTemplate.NewWarrantyTemplate("code-527", "name-588");
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
        MemoryWarrantyTemplateStoreFindsWhatItSaved();
    }
}
