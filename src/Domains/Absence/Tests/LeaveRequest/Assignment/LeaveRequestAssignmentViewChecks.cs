namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LeaveRequestAssignmentViewChecks
{
    public static void LeaveRequestAssignmentViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestAssignment.NewLeaveRequestAssignment("code-142", "title-740");
        var view = LeaveRequestAssignmentView.ToLeaveRequestAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestAssignmentView.ToLeaveRequestAssignmentViews(new List<LeaveRequestAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestAssignmentViewShowsTheStatusInCapitals();
    }
}
