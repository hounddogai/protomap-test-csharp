namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdaptiveTaxFormScoreViewChecks
{
    public static void AdaptiveTaxFormScoreViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveTaxFormScore.NewAdaptiveTaxFormScore("code-152", "name-68");
        var view = AdaptiveTaxFormScoreView.ToAdaptiveTaxFormScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveTaxFormScoreView.ToAdaptiveTaxFormScoreViews(new List<AdaptiveTaxFormScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveTaxFormScoreViewShowsTheStatusInCapitals();
    }
}
