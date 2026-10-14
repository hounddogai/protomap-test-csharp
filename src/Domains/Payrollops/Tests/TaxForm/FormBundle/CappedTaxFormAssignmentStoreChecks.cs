namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class CappedTaxFormAssignmentStoreChecks
{
    public static void MemoryCappedTaxFormAssignmentStoreFindsWhatItSaved()
    {
        var store = CappedTaxFormAssignmentStore.NewMemoryCappedTaxFormAssignmentStore();
        var item = CappedTaxFormAssignment.NewCappedTaxFormAssignment("code-253", "title-778");
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
        MemoryCappedTaxFormAssignmentStoreFindsWhatItSaved();
    }
}
