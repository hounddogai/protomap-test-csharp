namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PayRunBaselineStoreChecks
{
    public static void MemoryPayRunBaselineStoreFindsWhatItSaved()
    {
        var store = PayRunBaselineStore.NewMemoryPayRunBaselineStore();
        var item = PayRunBaseline.NewPayRunBaseline("code-860", "title-972");
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
        MemoryPayRunBaselineStoreFindsWhatItSaved();
    }
}
