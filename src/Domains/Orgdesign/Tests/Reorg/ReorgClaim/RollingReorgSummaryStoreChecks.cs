namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgClaim;

using Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

/// <summary>Checks of reorg claim code, run by the project's runner.</summary>
public static class RollingReorgSummaryStoreChecks
{
    public static void MemoryRollingReorgSummaryStoreFindsWhatItSaved()
    {
        var store = RollingReorgSummaryStore.NewMemoryRollingReorgSummaryStore();
        var item = RollingReorgSummary.NewRollingReorgSummary("code-752", "name-175");
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
        MemoryRollingReorgSummaryStoreFindsWhatItSaved();
    }
}
