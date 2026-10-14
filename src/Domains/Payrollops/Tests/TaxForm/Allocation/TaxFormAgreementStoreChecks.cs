namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAgreementStoreChecks
{
    public static void MemoryTaxFormAgreementStoreFindsWhatItSaved()
    {
        var store = TaxFormAgreementStore.NewMemoryTaxFormAgreementStore();
        var item = TaxFormAgreement.NewTaxFormAgreement("code-74", "title-745");
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
        MemoryTaxFormAgreementStoreFindsWhatItSaved();
    }
}
