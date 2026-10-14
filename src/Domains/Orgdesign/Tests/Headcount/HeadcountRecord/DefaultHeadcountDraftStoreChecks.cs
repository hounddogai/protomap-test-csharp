namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class DefaultHeadcountDraftStoreChecks
{
    public static void MemoryDefaultHeadcountDraftStoreFindsWhatItSaved()
    {
        var store = DefaultHeadcountDraftStore.NewMemoryDefaultHeadcountDraftStore();
        var item = DefaultHeadcountDraft.NewDefaultHeadcountDraft("code-35", "label-540");
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
        MemoryDefaultHeadcountDraftStoreFindsWhatItSaved();
    }
}
