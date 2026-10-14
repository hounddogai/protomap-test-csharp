namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class LeaveRequestScoreViewChecks
{
    public static void LeaveRequestScoreViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestScore.NewLeaveRequestScore("code-315", "title-787");
        var view = LeaveRequestScoreView.ToLeaveRequestScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestScoreView.ToLeaveRequestScoreViews(new List<LeaveRequestScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestScoreViewShowsTheStatusInCapitals();
    }
}
