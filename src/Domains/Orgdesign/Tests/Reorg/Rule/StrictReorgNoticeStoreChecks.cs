namespace Acme.Workforce.Orgdesign.Tests.Reorg.Rule;

using Acme.Workforce.Orgdesign.Reorg.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictReorgNoticeStoreChecks
{
    public static void MemoryStrictReorgNoticeStoreFindsWhatItSaved()
    {
        var store = StrictReorgNoticeStore.NewMemoryStrictReorgNoticeStore();
        var item = StrictReorgNotice.NewStrictReorgNotice("code-800", "label-717");
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
        MemoryStrictReorgNoticeStoreFindsWhatItSaved();
    }
}
