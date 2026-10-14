namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class GlobalReorgDraftStoreChecks
{
    public static void MemoryGlobalReorgDraftStoreFindsWhatItSaved()
    {
        var store = GlobalReorgDraftStore.NewMemoryGlobalReorgDraftStore();
        var item = GlobalReorgDraft.NewGlobalReorgDraft("code-556", "title-722");
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
        MemoryGlobalReorgDraftStoreFindsWhatItSaved();
    }
}
