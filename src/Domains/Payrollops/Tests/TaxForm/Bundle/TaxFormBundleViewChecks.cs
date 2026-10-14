namespace Acme.Workforce.Payrollops.Tests.TaxForm.Bundle;

using Acme.Workforce.Payrollops.TaxForm.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TaxFormBundleViewChecks
{
    public static void TaxFormBundleViewShowsTheStatusInCapitals()
    {
        var item = TaxFormBundle.NewTaxFormBundle("code-223", "name-500");
        var view = TaxFormBundleView.ToTaxFormBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormBundleView.ToTaxFormBundleViews(new List<TaxFormBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormBundleViewShowsTheStatusInCapitals();
    }
}
