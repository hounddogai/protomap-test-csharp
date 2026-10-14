namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class RollingLeaveRequestPreferenceViewChecks
{
    public static void RollingLeaveRequestPreferenceViewShowsTheStatusInCapitals()
    {
        var item = RollingLeaveRequestPreference.NewRollingLeaveRequestPreference("code-768", "label-356");
        var view = RollingLeaveRequestPreferenceView.ToRollingLeaveRequestPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingLeaveRequestPreferenceView.ToRollingLeaveRequestPreferenceViews(new List<RollingLeaveRequestPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingLeaveRequestPreferenceViewShowsTheStatusInCapitals();
    }
}
