namespace Acme.Workforce.Orgdesign.Tests.Reorg.Group;

using Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ReorgGroupStoreChecks
{
    public static void MemoryReorgGroupStoreFindsWhatItSaved()
    {
        var store = ReorgGroupStore.NewMemoryReorgGroupStore();
        var item = ReorgGroup.NewReorgGroup("code-128", "title-100");
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
        MemoryReorgGroupStoreFindsWhatItSaved();
    }
}
