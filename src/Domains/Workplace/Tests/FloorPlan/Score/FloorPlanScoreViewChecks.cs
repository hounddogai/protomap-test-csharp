namespace Acme.Workforce.Workplace.Tests.FloorPlan.Score;

using Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FloorPlanScoreViewChecks
{
    public static void FloorPlanScoreViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanScore.NewFloorPlanScore("code-415", "name-513");
        var view = FloorPlanScoreView.ToFloorPlanScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanScoreView.ToFloorPlanScoreViews(new List<FloorPlanScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanScoreViewShowsTheStatusInCapitals();
    }
}
