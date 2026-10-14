namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class DefaultReorgGroupStoreChecks
{
    public static void MemoryDefaultReorgGroupStoreFindsWhatItSaved()
    {
        var store = DefaultReorgGroupStore.NewMemoryDefaultReorgGroupStore();
        var item = DefaultReorgGroup.NewDefaultReorgGroup("code-997", "label-95");
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
        MemoryDefaultReorgGroupStoreFindsWhatItSaved();
    }
}
