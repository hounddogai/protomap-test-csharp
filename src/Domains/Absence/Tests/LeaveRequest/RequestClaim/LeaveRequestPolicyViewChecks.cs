namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class LeaveRequestPolicyViewChecks
{
    public static void LeaveRequestPolicyViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestPolicy.NewLeaveRequestPolicy("code-292", "label-859");
        var view = LeaveRequestPolicyView.ToLeaveRequestPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestPolicyView.ToLeaveRequestPolicyViews(new List<LeaveRequestPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestPolicyViewShowsTheStatusInCapitals();
    }
}
