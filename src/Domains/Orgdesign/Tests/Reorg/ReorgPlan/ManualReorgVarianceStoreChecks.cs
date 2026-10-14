namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class ManualReorgVarianceStoreChecks
{
    public static void MemoryManualReorgVarianceStoreFindsWhatItSaved()
    {
        var store = ManualReorgVarianceStore.NewMemoryManualReorgVarianceStore();
        var item = ManualReorgVariance.NewManualReorgVariance("code-192", "name-703");
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
        MemoryManualReorgVarianceStoreFindsWhatItSaved();
    }
}
