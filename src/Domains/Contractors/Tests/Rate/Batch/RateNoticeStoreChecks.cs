namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateNoticeStoreChecks
{
    public static void MemoryRateNoticeStoreFindsWhatItSaved()
    {
        var store = RateNoticeStore.NewMemoryRateNoticeStore();
        var item = RateNotice.NewRateNotice("code-383", "label-716");
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
        MemoryRateNoticeStoreFindsWhatItSaved();
    }
}
