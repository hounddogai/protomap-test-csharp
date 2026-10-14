namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopBaselineStoreChecks
{
    public static void MemoryLaptopBaselineStoreFindsWhatItSaved()
    {
        var store = LaptopBaselineStore.NewMemoryLaptopBaselineStore();
        var item = LaptopBaseline.NewLaptopBaseline("code-593", "label-612");
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
        MemoryLaptopBaselineStoreFindsWhatItSaved();
    }
}
