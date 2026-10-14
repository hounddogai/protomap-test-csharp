namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DeductionAuditStoreChecks
{
    public static void MemoryDeductionAuditStoreFindsWhatItSaved()
    {
        var store = DeductionAuditStore.NewMemoryDeductionAuditStore();
        var item = DeductionAudit.NewDeductionAudit("code-688", "name-676");
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
        MemoryDeductionAuditStoreFindsWhatItSaved();
    }
}
