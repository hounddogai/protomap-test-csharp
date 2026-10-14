namespace Acme.Workforce.Payrollops.Tests.TaxForm.Bundle;

using Acme.Workforce.Payrollops.TaxForm.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TaxFormTargetStoreChecks
{
    public static void MemoryTaxFormTargetStoreFindsWhatItSaved()
    {
        var store = TaxFormTargetStore.NewMemoryTaxFormTargetStore();
        var item = TaxFormTarget.NewTaxFormTarget("code-679", "title-912");
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
        MemoryTaxFormTargetStoreFindsWhatItSaved();
    }
}
