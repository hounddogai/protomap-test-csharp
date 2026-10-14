namespace Acme.Workforce.Payrollops.Tests.TaxForm.Threshold;

using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ManualTaxFormAuditStoreChecks
{
    public static void MemoryManualTaxFormAuditStoreFindsWhatItSaved()
    {
        var store = ManualTaxFormAuditStore.NewMemoryManualTaxFormAuditStore();
        var item = ManualTaxFormAudit.NewManualTaxFormAudit("code-159", "name-971");
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
        MemoryManualTaxFormAuditStoreFindsWhatItSaved();
    }
}
