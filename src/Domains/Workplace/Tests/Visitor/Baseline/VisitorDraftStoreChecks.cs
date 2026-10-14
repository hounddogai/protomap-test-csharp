namespace Acme.Workforce.Workplace.Tests.Visitor.Baseline;

using Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class VisitorDraftStoreChecks
{
    public static void MemoryVisitorDraftStoreFindsWhatItSaved()
    {
        var store = VisitorDraftStore.NewMemoryVisitorDraftStore();
        var item = VisitorDraft.NewVisitorDraft("code-892", "title-877");
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
        MemoryVisitorDraftStoreFindsWhatItSaved();
    }
}
