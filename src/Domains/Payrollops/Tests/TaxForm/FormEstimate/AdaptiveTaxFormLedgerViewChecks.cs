namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class AdaptiveTaxFormLedgerViewChecks
{
    public static void AdaptiveTaxFormLedgerViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveTaxFormLedger.NewAdaptiveTaxFormLedger("code-782", "label-956");
        var view = AdaptiveTaxFormLedgerView.ToAdaptiveTaxFormLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveTaxFormLedgerView.ToAdaptiveTaxFormLedgerViews(new List<AdaptiveTaxFormLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveTaxFormLedgerViewShowsTheStatusInCapitals();
    }
}
