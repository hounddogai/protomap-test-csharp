namespace Acme.Workforce.Compliance.Tests.PolicyAck.Proposal;

using Acme.Workforce.Compliance.PolicyAck.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RollingPolicyAckEstimateViewChecks
{
    public static void RollingPolicyAckEstimateViewShowsTheStatusInCapitals()
    {
        var item = RollingPolicyAckEstimate.NewRollingPolicyAckEstimate("code-286", "title-292");
        var view = RollingPolicyAckEstimateView.ToRollingPolicyAckEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingPolicyAckEstimateView.ToRollingPolicyAckEstimateViews(new List<RollingPolicyAckEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingPolicyAckEstimateViewShowsTheStatusInCapitals();
    }
}
