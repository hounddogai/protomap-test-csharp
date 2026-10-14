namespace Acme.Workforce.Orgdesign.Tests.Reorg.Group;

using Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ReorgBundleStoreChecks
{
    public static void MemoryReorgBundleStoreFindsWhatItSaved()
    {
        var store = ReorgBundleStore.NewMemoryReorgBundleStore();
        var item = ReorgBundle.NewReorgBundle("code-728", "title-746");
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
        MemoryReorgBundleStoreFindsWhatItSaved();
    }
}
