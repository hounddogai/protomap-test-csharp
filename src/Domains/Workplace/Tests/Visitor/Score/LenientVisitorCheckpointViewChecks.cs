namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientVisitorCheckpointViewChecks
{
    public static void LenientVisitorCheckpointViewShowsTheStatusInCapitals()
    {
        var item = LenientVisitorCheckpoint.NewLenientVisitorCheckpoint("code-869", "name-199");
        var view = LenientVisitorCheckpointView.ToLenientVisitorCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientVisitorCheckpointView.ToLenientVisitorCheckpointViews(new List<LenientVisitorCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientVisitorCheckpointViewShowsTheStatusInCapitals();
    }
}
