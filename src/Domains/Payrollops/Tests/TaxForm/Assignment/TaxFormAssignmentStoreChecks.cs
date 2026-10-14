namespace Acme.Workforce.Payrollops.Tests.TaxForm.Assignment;

using Acme.Workforce.Payrollops.TaxForm.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TaxFormAssignmentStoreChecks
{
    public static void MemoryTaxFormAssignmentStoreFindsWhatItSaved()
    {
        var store = TaxFormAssignmentStore.NewMemoryTaxFormAssignmentStore();
        var item = TaxFormAssignment.NewTaxFormAssignment("code-111", "title-385");
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
        MemoryTaxFormAssignmentStoreFindsWhatItSaved();
    }
}
