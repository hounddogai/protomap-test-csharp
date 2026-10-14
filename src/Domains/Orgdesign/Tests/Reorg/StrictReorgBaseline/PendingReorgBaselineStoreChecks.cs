namespace Acme.Workforce.Orgdesign.Tests.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Checks of strict reorg baseline code, run by the project's runner.</summary>
public static class PendingReorgBaselineStoreChecks
{
    public static void MemoryPendingReorgBaselineStoreFindsWhatItSaved()
    {
        var store = PendingReorgBaselineStore.NewMemoryPendingReorgBaselineStore();
        var item = PendingReorgBaseline.NewPendingReorgBaseline("code-565", "label-283");
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
        MemoryPendingReorgBaselineStoreFindsWhatItSaved();
    }
}
