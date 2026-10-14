namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantNoticeStoreChecks
{
    public static void MemoryEquityGrantNoticeStoreFindsWhatItSaved()
    {
        var store = EquityGrantNoticeStore.NewMemoryEquityGrantNoticeStore();
        var item = EquityGrantNotice.NewEquityGrantNotice("code-617", "title-243");
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
        MemoryEquityGrantNoticeStoreFindsWhatItSaved();
    }
}
