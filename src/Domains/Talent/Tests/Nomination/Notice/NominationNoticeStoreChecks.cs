namespace Acme.Workforce.Talent.Tests.Nomination.Notice;

using Acme.Workforce.Talent.Nomination.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class NominationNoticeStoreChecks
{
    public static void MemoryNominationNoticeStoreFindsWhatItSaved()
    {
        var store = NominationNoticeStore.NewMemoryNominationNoticeStore();
        var item = NominationNotice.NewNominationNotice("code-930", "title-886");
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
        MemoryNominationNoticeStoreFindsWhatItSaved();
    }
}
