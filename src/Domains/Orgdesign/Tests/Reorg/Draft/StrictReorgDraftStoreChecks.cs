namespace Acme.Workforce.Orgdesign.Tests.Reorg.Draft;

using Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictReorgDraftStoreChecks
{
    public static void MemoryStrictReorgDraftStoreFindsWhatItSaved()
    {
        var store = StrictReorgDraftStore.NewMemoryStrictReorgDraftStore();
        var item = StrictReorgDraft.NewStrictReorgDraft("code-736", "title-703");
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
        MemoryStrictReorgDraftStoreFindsWhatItSaved();
    }
}
