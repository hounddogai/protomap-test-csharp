namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class LeaveRequestSegmentViewChecks
{
    public static void LeaveRequestSegmentViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestSegment.NewLeaveRequestSegment("code-92", "title-733");
        var view = LeaveRequestSegmentView.ToLeaveRequestSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestSegmentView.ToLeaveRequestSegmentViews(new List<LeaveRequestSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestSegmentViewShowsTheStatusInCapitals();
    }
}
