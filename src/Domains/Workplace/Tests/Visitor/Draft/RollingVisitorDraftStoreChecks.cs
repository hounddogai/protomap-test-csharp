namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RollingVisitorDraftStoreChecks
{
    public static void MemoryRollingVisitorDraftStoreFindsWhatItSaved()
    {
        var store = RollingVisitorDraftStore.NewMemoryRollingVisitorDraftStore();
        var item = RollingVisitorDraft.NewRollingVisitorDraft("code-72", "title-686");
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
        MemoryRollingVisitorDraftStoreFindsWhatItSaved();
    }
}
