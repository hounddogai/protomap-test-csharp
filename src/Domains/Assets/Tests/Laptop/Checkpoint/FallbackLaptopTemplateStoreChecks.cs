namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FallbackLaptopTemplateStoreChecks
{
    public static void MemoryFallbackLaptopTemplateStoreFindsWhatItSaved()
    {
        var store = FallbackLaptopTemplateStore.NewMemoryFallbackLaptopTemplateStore();
        var item = FallbackLaptopTemplate.NewFallbackLaptopTemplate("code-295", "label-591");
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
        MemoryFallbackLaptopTemplateStoreFindsWhatItSaved();
    }
}
