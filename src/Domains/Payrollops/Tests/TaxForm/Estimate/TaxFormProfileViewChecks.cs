namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TaxFormProfileViewChecks
{
    public static void TaxFormProfileViewShowsTheStatusInCapitals()
    {
        var item = TaxFormProfile.NewTaxFormProfile("code-945", "name-76");
        var view = TaxFormProfileView.ToTaxFormProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormProfileView.ToTaxFormProfileViews(new List<TaxFormProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormProfileViewShowsTheStatusInCapitals();
    }
}
