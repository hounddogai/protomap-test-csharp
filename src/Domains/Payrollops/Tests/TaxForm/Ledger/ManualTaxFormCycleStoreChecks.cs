namespace Acme.Workforce.Payrollops.Tests.TaxForm.Ledger;

using Acme.Workforce.Payrollops.TaxForm.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ManualTaxFormCycleStoreChecks
{
    public static void MemoryManualTaxFormCycleStoreFindsWhatItSaved()
    {
        var store = ManualTaxFormCycleStore.NewMemoryManualTaxFormCycleStore();
        var item = ManualTaxFormCycle.NewManualTaxFormCycle("code-902", "name-723");
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
        MemoryManualTaxFormCycleStoreFindsWhatItSaved();
    }
}
