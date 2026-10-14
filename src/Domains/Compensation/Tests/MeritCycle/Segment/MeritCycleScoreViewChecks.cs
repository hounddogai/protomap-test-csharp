namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class MeritCycleScoreViewChecks
{
    public static void MeritCycleScoreViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleScore.NewMeritCycleScore("code-739", "name-622");
        var view = MeritCycleScoreView.ToMeritCycleScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleScoreView.ToMeritCycleScoreViews(new List<MeritCycleScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleScoreViewShowsTheStatusInCapitals();
    }
}
