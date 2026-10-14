namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PoolDraftStoreChecks
{
    public static void MemoryPoolDraftStoreFindsWhatItSaved()
    {
        var store = PoolDraftStore.NewMemoryPoolDraftStore();
        var item = PoolDraft.NewPoolDraft("code-299", "title-374");
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
        MemoryPoolDraftStoreFindsWhatItSaved();
    }
}
