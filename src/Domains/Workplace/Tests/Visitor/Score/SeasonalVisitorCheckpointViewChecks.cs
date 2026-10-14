namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SeasonalVisitorCheckpointViewChecks
{
    public static void SeasonalVisitorCheckpointViewShowsTheStatusInCapitals()
    {
        var item = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint("code-809", "title-56");
        var view = SeasonalVisitorCheckpointView.ToSeasonalVisitorCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalVisitorCheckpointView.ToSeasonalVisitorCheckpointViews(new List<SeasonalVisitorCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalVisitorCheckpointViewShowsTheStatusInCapitals();
    }
}
