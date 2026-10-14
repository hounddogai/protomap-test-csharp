namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LenientAssignmentScoreViewChecks
{
    public static void LenientAssignmentScoreViewShowsTheStatusInCapitals()
    {
        var item = LenientAssignmentScore.NewLenientAssignmentScore("code-227", "label-471");
        var view = LenientAssignmentScoreView.ToLenientAssignmentScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientAssignmentScoreView.ToLenientAssignmentScoreViews(new List<LenientAssignmentScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientAssignmentScoreViewShowsTheStatusInCapitals();
    }
}
