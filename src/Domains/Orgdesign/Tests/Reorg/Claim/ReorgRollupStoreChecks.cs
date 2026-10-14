namespace Acme.Workforce.Orgdesign.Tests.Reorg.Claim;

using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ReorgRollupStoreChecks
{
    public static void MemoryReorgRollupStoreFindsWhatItSaved()
    {
        var store = ReorgRollupStore.NewMemoryReorgRollupStore();
        var item = ReorgRollup.NewReorgRollup("code-83", "name-133");
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
        MemoryReorgRollupStoreFindsWhatItSaved();
    }
}
