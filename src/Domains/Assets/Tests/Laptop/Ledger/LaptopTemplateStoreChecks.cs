namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopTemplateStoreChecks
{
    public static void MemoryLaptopTemplateStoreFindsWhatItSaved()
    {
        var store = LaptopTemplateStore.NewMemoryLaptopTemplateStore();
        var item = LaptopTemplate.NewLaptopTemplate("code-84", "title-859");
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
        MemoryLaptopTemplateStoreFindsWhatItSaved();
    }
}
