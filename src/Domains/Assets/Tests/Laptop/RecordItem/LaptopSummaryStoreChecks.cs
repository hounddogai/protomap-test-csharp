namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopSummaryStoreChecks
{
    public static void MemoryLaptopSummaryStoreFindsWhatItSaved()
    {
        var store = LaptopSummaryStore.NewMemoryLaptopSummaryStore();
        var item = LaptopSummary.NewLaptopSummary("code-336", "title-906");
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
        MemoryLaptopSummaryStoreFindsWhatItSaved();
    }
}
