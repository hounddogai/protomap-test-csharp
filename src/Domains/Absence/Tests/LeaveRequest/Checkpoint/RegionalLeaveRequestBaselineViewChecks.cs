namespace Acme.Workforce.Absence.Tests.LeaveRequest.Checkpoint;

using Acme.Workforce.Absence.LeaveRequest.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalLeaveRequestBaselineViewChecks
{
    public static void RegionalLeaveRequestBaselineViewShowsTheStatusInCapitals()
    {
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("code-765", "name-34");
        var view = RegionalLeaveRequestBaselineView.ToRegionalLeaveRequestBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalLeaveRequestBaselineView.ToRegionalLeaveRequestBaselineViews(new List<RegionalLeaveRequestBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalLeaveRequestBaselineViewShowsTheStatusInCapitals();
    }
}
