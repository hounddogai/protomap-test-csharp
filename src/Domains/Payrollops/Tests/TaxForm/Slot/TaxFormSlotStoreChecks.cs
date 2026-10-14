namespace Acme.Workforce.Payrollops.Tests.TaxForm.Slot;

using Acme.Workforce.Payrollops.TaxForm.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class TaxFormSlotStoreChecks
{
    public static void MemoryTaxFormSlotStoreFindsWhatItSaved()
    {
        var store = TaxFormSlotStore.NewMemoryTaxFormSlotStore();
        var item = TaxFormSlot.NewTaxFormSlot("code-546", "title-167");
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
        MemoryTaxFormSlotStoreFindsWhatItSaved();
    }
}
