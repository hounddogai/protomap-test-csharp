namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyLaptopTemplateStoreChecks
{
    public static void MemoryLegacyLaptopTemplateStoreFindsWhatItSaved()
    {
        var store = LegacyLaptopTemplateStore.NewMemoryLegacyLaptopTemplateStore();
        var item = LegacyLaptopTemplate.NewLegacyLaptopTemplate("code-988", "title-380");
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
        MemoryLegacyLaptopTemplateStoreFindsWhatItSaved();
    }
}
