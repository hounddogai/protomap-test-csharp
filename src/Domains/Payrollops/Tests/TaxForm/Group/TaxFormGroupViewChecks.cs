namespace Acme.Workforce.Payrollops.Tests.TaxForm.Group;

using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class TaxFormGroupViewChecks
{
    public static void TaxFormGroupViewShowsTheStatusInCapitals()
    {
        var item = TaxFormGroup.NewTaxFormGroup("code-261", "label-563");
        var view = TaxFormGroupView.ToTaxFormGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormGroupView.ToTaxFormGroupViews(new List<TaxFormGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormGroupViewShowsTheStatusInCapitals();
    }
}
