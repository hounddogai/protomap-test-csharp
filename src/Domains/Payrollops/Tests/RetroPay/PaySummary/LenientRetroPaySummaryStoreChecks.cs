namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class LenientRetroPaySummaryStoreChecks
{
    public static void MemoryLenientRetroPaySummaryStoreFindsWhatItSaved()
    {
        var store = LenientRetroPaySummaryStore.NewMemoryLenientRetroPaySummaryStore();
        var item = LenientRetroPaySummary.NewLenientRetroPaySummary("code-387", "name-362");
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
        MemoryLenientRetroPaySummaryStoreFindsWhatItSaved();
    }
}
