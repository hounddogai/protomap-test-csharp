namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalVisitorDraftStoreChecks
{
    public static void MemoryGlobalVisitorDraftStoreFindsWhatItSaved()
    {
        var store = GlobalVisitorDraftStore.NewMemoryGlobalVisitorDraftStore();
        var item = GlobalVisitorDraft.NewGlobalVisitorDraft("code-261", "title-965");
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
        MemoryGlobalVisitorDraftStoreFindsWhatItSaved();
    }
}
