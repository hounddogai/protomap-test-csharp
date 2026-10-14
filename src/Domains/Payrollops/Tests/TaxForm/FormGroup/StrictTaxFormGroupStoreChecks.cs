namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class StrictTaxFormGroupStoreChecks
{
    public static void MemoryStrictTaxFormGroupStoreFindsWhatItSaved()
    {
        var store = StrictTaxFormGroupStore.NewMemoryStrictTaxFormGroupStore();
        var item = StrictTaxFormGroup.NewStrictTaxFormGroup("code-342", "label-670");
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
        MemoryStrictTaxFormGroupStoreFindsWhatItSaved();
    }
}
