namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PayRunAdjustmentStoreChecks
{
    public static void MemoryPayRunAdjustmentStoreFindsWhatItSaved()
    {
        var store = PayRunAdjustmentStore.NewMemoryPayRunAdjustmentStore();
        var item = PayRunAdjustment.NewPayRunAdjustment("code-746", "name-841");
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
        MemoryPayRunAdjustmentStoreFindsWhatItSaved();
    }
}
