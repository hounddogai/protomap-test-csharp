namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TaxFormProfileStoreChecks
{
    public static void MemoryTaxFormProfileStoreFindsWhatItSaved()
    {
        var store = TaxFormProfileStore.NewMemoryTaxFormProfileStore();
        var item = TaxFormProfile.NewTaxFormProfile("code-945", "name-76");
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
        MemoryTaxFormProfileStoreFindsWhatItSaved();
    }
}
