namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ManualReorgMetricStoreChecks
{
    public static void MemoryManualReorgMetricStoreFindsWhatItSaved()
    {
        var store = ManualReorgMetricStore.NewMemoryManualReorgMetricStore();
        var item = ManualReorgMetric.NewManualReorgMetric("code-963", "title-168");
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
        MemoryManualReorgMetricStoreFindsWhatItSaved();
    }
}
