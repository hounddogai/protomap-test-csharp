namespace Acme.Workforce.Payrollops.Tests.PayRun.RunRecord;

using Acme.Workforce.Payrollops.PayRun.RunRecord;

/// <summary>Checks of run record code, run by the project's runner.</summary>
public static class PayRunEstimateStoreChecks
{
    public static void MemoryPayRunEstimateStoreFindsWhatItSaved()
    {
        var store = PayRunEstimateStore.NewMemoryPayRunEstimateStore();
        var item = PayRunEstimate.NewPayRunEstimate("code-930", "name-550");
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
        MemoryPayRunEstimateStoreFindsWhatItSaved();
    }
}
