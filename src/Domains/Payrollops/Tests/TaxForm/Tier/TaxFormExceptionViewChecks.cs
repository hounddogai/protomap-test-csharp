namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TaxFormExceptionViewChecks
{
    public static void TaxFormExceptionViewShowsTheStatusInCapitals()
    {
        var item = TaxFormException.NewTaxFormException("code-232", "title-21");
        var view = TaxFormExceptionView.ToTaxFormExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormExceptionView.ToTaxFormExceptionViews(new List<TaxFormException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormExceptionViewShowsTheStatusInCapitals();
    }
}
