namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ManualLeaveRequestScoreViewChecks
{
    public static void ManualLeaveRequestScoreViewShowsTheStatusInCapitals()
    {
        var item = ManualLeaveRequestScore.NewManualLeaveRequestScore("code-475", "name-841");
        var view = ManualLeaveRequestScoreView.ToManualLeaveRequestScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualLeaveRequestScoreView.ToManualLeaveRequestScoreViews(new List<ManualLeaveRequestScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualLeaveRequestScoreViewShowsTheStatusInCapitals();
    }
}
