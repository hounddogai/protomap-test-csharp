namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class RegionalReorgWindowStoreChecks
{
    public static void MemoryRegionalReorgWindowStoreFindsWhatItSaved()
    {
        var store = RegionalReorgWindowStore.NewMemoryRegionalReorgWindowStore();
        var item = RegionalReorgWindow.NewRegionalReorgWindow("code-644", "label-979");
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
        MemoryRegionalReorgWindowStoreFindsWhatItSaved();
    }
}
