namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientPoolDraftStoreChecks
{
    public static void MemoryLenientPoolDraftStoreFindsWhatItSaved()
    {
        var store = LenientPoolDraftStore.NewMemoryLenientPoolDraftStore();
        var item = LenientPoolDraft.NewLenientPoolDraft("code-535", "title-477");
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
        MemoryLenientPoolDraftStoreFindsWhatItSaved();
    }
}
