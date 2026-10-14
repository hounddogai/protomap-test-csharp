namespace Acme.Workforce.Payrollops.Tests.TaxForm.Bundle;

using Acme.Workforce.Payrollops.TaxForm.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TaxFormTargetViewChecks
{
    public static void TaxFormTargetViewShowsTheStatusInCapitals()
    {
        var item = TaxFormTarget.NewTaxFormTarget("code-679", "title-912");
        var view = TaxFormTargetView.ToTaxFormTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormTargetView.ToTaxFormTargetViews(new List<TaxFormTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormTargetViewShowsTheStatusInCapitals();
    }
}
