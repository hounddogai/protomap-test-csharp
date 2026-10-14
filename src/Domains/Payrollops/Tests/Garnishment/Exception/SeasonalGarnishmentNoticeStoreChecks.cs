namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SeasonalGarnishmentNoticeStoreChecks
{
    public static void MemorySeasonalGarnishmentNoticeStoreFindsWhatItSaved()
    {
        var store = SeasonalGarnishmentNoticeStore.NewMemorySeasonalGarnishmentNoticeStore();
        var item = SeasonalGarnishmentNotice.NewSeasonalGarnishmentNotice("code-855", "label-764");
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
        MemorySeasonalGarnishmentNoticeStoreFindsWhatItSaved();
    }
}
