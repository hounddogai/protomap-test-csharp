namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TaxFormDraftViewChecks
{
    public static void TaxFormDraftViewShowsTheStatusInCapitals()
    {
        var item = TaxFormDraft.NewTaxFormDraft("code-297", "name-423");
        var view = TaxFormDraftView.ToTaxFormDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormDraftView.ToTaxFormDraftViews(new List<TaxFormDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormDraftViewShowsTheStatusInCapitals();
    }
}
