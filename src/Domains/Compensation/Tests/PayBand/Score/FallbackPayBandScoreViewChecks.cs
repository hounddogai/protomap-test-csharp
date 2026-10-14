namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FallbackPayBandScoreViewChecks
{
    public static void FallbackPayBandScoreViewShowsTheStatusInCapitals()
    {
        var item = FallbackPayBandScore.NewFallbackPayBandScore("code-180", "name-235");
        var view = FallbackPayBandScoreView.ToFallbackPayBandScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPayBandScoreView.ToFallbackPayBandScoreViews(new List<FallbackPayBandScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPayBandScoreViewShowsTheStatusInCapitals();
    }
}
