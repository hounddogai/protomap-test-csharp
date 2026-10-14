namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class StrictAssignmentScoreViewChecks
{
    public static void StrictAssignmentScoreViewShowsTheStatusInCapitals()
    {
        var item = StrictAssignmentScore.NewStrictAssignmentScore("code-461", "name-530");
        var view = StrictAssignmentScoreView.ToStrictAssignmentScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictAssignmentScoreView.ToStrictAssignmentScoreViews(new List<StrictAssignmentScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictAssignmentScoreViewShowsTheStatusInCapitals();
    }
}
