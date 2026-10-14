namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GarnishmentNoticeStoreChecks
{
    public static void MemoryGarnishmentNoticeStoreFindsWhatItSaved()
    {
        var store = GarnishmentNoticeStore.NewMemoryGarnishmentNoticeStore();
        var item = GarnishmentNotice.NewGarnishmentNotice("code-192", "name-533");
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
        MemoryGarnishmentNoticeStoreFindsWhatItSaved();
    }
}
