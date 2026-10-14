namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CappedMeritCycleScoreViewChecks
{
    public static void CappedMeritCycleScoreViewShowsTheStatusInCapitals()
    {
        var item = CappedMeritCycleScore.NewCappedMeritCycleScore("code-465", "title-529");
        var view = CappedMeritCycleScoreView.ToCappedMeritCycleScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedMeritCycleScoreView.ToCappedMeritCycleScoreViews(new List<CappedMeritCycleScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedMeritCycleScoreViewShowsTheStatusInCapitals();
    }
}
