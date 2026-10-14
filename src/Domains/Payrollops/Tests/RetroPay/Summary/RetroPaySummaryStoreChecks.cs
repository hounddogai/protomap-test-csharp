namespace Acme.Workforce.Payrollops.Tests.RetroPay.Summary;

using Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RetroPaySummaryStoreChecks
{
    public static void MemoryRetroPaySummaryStoreFindsWhatItSaved()
    {
        var store = RetroPaySummaryStore.NewMemoryRetroPaySummaryStore();
        var item = RetroPaySummary.NewRetroPaySummary("code-852", "label-960");
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
        MemoryRetroPaySummaryStoreFindsWhatItSaved();
    }
}
