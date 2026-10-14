namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class LegacyReorgSummaryStoreChecks
{
    public static void MemoryLegacyReorgSummaryStoreFindsWhatItSaved()
    {
        var store = LegacyReorgSummaryStore.NewMemoryLegacyReorgSummaryStore();
        var item = LegacyReorgSummary.NewLegacyReorgSummary("code-711", "title-16");
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
        MemoryLegacyReorgSummaryStoreFindsWhatItSaved();
    }
}
