namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedPayslipDraftStoreChecks
{
    public static void MemoryWeightedPayslipDraftStoreFindsWhatItSaved()
    {
        var store = WeightedPayslipDraftStore.NewMemoryWeightedPayslipDraftStore();
        var item = WeightedPayslipDraft.NewWeightedPayslipDraft("code-738", "label-979");
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
        MemoryWeightedPayslipDraftStoreFindsWhatItSaved();
    }
}
