namespace Acme.Workforce.Payrollops.Tests.TaxForm.Template;

using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TaxFormRollupViewChecks
{
    public static void TaxFormRollupViewShowsTheStatusInCapitals()
    {
        var item = TaxFormRollup.NewTaxFormRollup("code-891", "title-149");
        var view = TaxFormRollupView.ToTaxFormRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormRollupView.ToTaxFormRollupViews(new List<TaxFormRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormRollupViewShowsTheStatusInCapitals();
    }
}
