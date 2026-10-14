namespace Acme.Workforce.Payrollops.Tests.Deduction.Quota;

using Acme.Workforce.Payrollops.Deduction.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DeductionQuotaStoreChecks
{
    public static void MemoryDeductionQuotaStoreFindsWhatItSaved()
    {
        var store = DeductionQuotaStore.NewMemoryDeductionQuotaStore();
        var item = DeductionQuota.NewDeductionQuota("code-12", "name-925");
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
        MemoryDeductionQuotaStoreFindsWhatItSaved();
    }
}
