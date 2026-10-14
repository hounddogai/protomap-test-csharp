namespace Acme.Workforce.Compliance.Tests.PolicyAck.Bundle;

using Acme.Workforce.Compliance.PolicyAck.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PolicyAckEstimateViewChecks
{
    public static void PolicyAckEstimateViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckEstimate.NewPolicyAckEstimate("code-653", "name-366");
        var view = PolicyAckEstimateView.ToPolicyAckEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckEstimateView.ToPolicyAckEstimateViews(new List<PolicyAckEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckEstimateViewShowsTheStatusInCapitals();
    }
}
