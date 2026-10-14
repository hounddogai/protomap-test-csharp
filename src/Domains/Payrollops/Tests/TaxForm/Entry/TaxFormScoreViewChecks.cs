namespace Acme.Workforce.Payrollops.Tests.TaxForm.Entry;

using Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TaxFormScoreViewChecks
{
    public static void TaxFormScoreViewShowsTheStatusInCapitals()
    {
        var item = TaxFormScore.NewTaxFormScore("code-276", "label-659");
        var view = TaxFormScoreView.ToTaxFormScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormScoreView.ToTaxFormScoreViews(new List<TaxFormScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormScoreViewShowsTheStatusInCapitals();
    }
}
