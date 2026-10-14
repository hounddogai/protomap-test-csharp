namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class StrictTaxFormGroupViewChecks
{
    public static void StrictTaxFormGroupViewShowsTheStatusInCapitals()
    {
        var item = StrictTaxFormGroup.NewStrictTaxFormGroup("code-342", "label-670");
        var view = StrictTaxFormGroupView.ToStrictTaxFormGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictTaxFormGroupView.ToStrictTaxFormGroupViews(new List<StrictTaxFormGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictTaxFormGroupViewShowsTheStatusInCapitals();
    }
}
