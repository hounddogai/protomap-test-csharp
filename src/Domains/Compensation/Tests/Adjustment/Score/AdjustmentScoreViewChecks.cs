namespace Acme.Workforce.Compensation.Tests.Adjustment.Score;

using Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdjustmentScoreViewChecks
{
    public static void AdjustmentScoreViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentScore.NewAdjustmentScore("code-284", "title-374");
        var view = AdjustmentScoreView.ToAdjustmentScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentScoreView.ToAdjustmentScoreViews(new List<AdjustmentScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentScoreViewShowsTheStatusInCapitals();
    }
}
