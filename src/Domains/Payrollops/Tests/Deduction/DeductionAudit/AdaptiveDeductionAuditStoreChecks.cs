namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class AdaptiveDeductionAuditStoreChecks
{
    public static void MemoryAdaptiveDeductionAuditStoreFindsWhatItSaved()
    {
        var store = AdaptiveDeductionAuditStore.NewMemoryAdaptiveDeductionAuditStore();
        var item = AdaptiveDeductionAudit.NewAdaptiveDeductionAudit("code-334", "label-807");
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
        MemoryAdaptiveDeductionAuditStoreFindsWhatItSaved();
    }
}
