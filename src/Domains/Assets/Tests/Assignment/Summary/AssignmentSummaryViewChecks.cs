namespace Acme.Workforce.Assets.Tests.Assignment.Summary;

using Acme.Workforce.Assets.Assignment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssignmentSummaryViewChecks
{
    public static void AssignmentSummaryViewShowsTheStatusInCapitals()
    {
        var item = AssignmentSummary.NewAssignmentSummary("code-279", "name-198");
        var view = AssignmentSummaryView.ToAssignmentSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentSummaryView.ToAssignmentSummaryViews(new List<AssignmentSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentSummaryViewShowsTheStatusInCapitals();
    }
}
