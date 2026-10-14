namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class AdaptiveLeaveRequestPolicyViewChecks
{
    public static void AdaptiveLeaveRequestPolicyViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("code-562", "name-723");
        var view = AdaptiveLeaveRequestPolicyView.ToAdaptiveLeaveRequestPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveLeaveRequestPolicyView.ToAdaptiveLeaveRequestPolicyViews(new List<AdaptiveLeaveRequestPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveLeaveRequestPolicyViewShowsTheStatusInCapitals();
    }
}
