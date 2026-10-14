namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedLeaveRequestBaselineViewChecks
{
    public static void CappedLeaveRequestBaselineViewShowsTheStatusInCapitals()
    {
        var item = CappedLeaveRequestBaseline.NewCappedLeaveRequestBaseline("code-560", "title-383");
        var view = CappedLeaveRequestBaselineView.ToCappedLeaveRequestBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedLeaveRequestBaselineView.ToCappedLeaveRequestBaselineViews(new List<CappedLeaveRequestBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedLeaveRequestBaselineViewShowsTheStatusInCapitals();
    }
}
