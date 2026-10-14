namespace Acme.Workforce.Orgdesign.Tests.Reorg.Baseline;

using Acme.Workforce.Orgdesign.Reorg.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ReorgBaselineStoreChecks
{
    public static void MemoryReorgBaselineStoreFindsWhatItSaved()
    {
        var store = ReorgBaselineStore.NewMemoryReorgBaselineStore();
        var item = ReorgBaseline.NewReorgBaseline("code-977", "title-547");
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
        MemoryReorgBaselineStoreFindsWhatItSaved();
    }
}
