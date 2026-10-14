namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class TaxFormWindowViewChecks
{
    public static void TaxFormWindowViewShowsTheStatusInCapitals()
    {
        var item = TaxFormWindow.NewTaxFormWindow("code-250", "name-515");
        var view = TaxFormWindowView.ToTaxFormWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormWindowView.ToTaxFormWindowViews(new List<TaxFormWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormWindowViewShowsTheStatusInCapitals();
    }
}
