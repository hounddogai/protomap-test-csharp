namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestMappingViewChecks
{
    public static void ScheduledLeaveRequestMappingViewShowsTheStatusInCapitals()
    {
        var item = ScheduledLeaveRequestMapping.NewScheduledLeaveRequestMapping("code-34", "title-965");
        var view = ScheduledLeaveRequestMappingView.ToScheduledLeaveRequestMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledLeaveRequestMappingView.ToScheduledLeaveRequestMappingViews(new List<ScheduledLeaveRequestMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestMappingViewShowsTheStatusInCapitals();
    }
}
