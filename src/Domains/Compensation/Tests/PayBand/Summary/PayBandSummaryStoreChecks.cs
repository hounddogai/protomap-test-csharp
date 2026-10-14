namespace Acme.Workforce.Compensation.Tests.PayBand.Summary;

using Acme.Workforce.Compensation.PayBand.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PayBandSummaryStoreChecks
{
    public static void MemoryPayBandSummaryStoreFindsWhatItSaved()
    {
        var store = PayBandSummaryStore.NewMemoryPayBandSummaryStore();
        var item = PayBandSummary.NewPayBandSummary("code-18", "title-11");
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
        MemoryPayBandSummaryStoreFindsWhatItSaved();
    }
}
