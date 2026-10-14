namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PolicyAckClaimViewChecks
{
    public static void PolicyAckClaimViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckClaim.NewPolicyAckClaim("code-695", "title-531");
        var view = PolicyAckClaimView.ToPolicyAckClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckClaimView.ToPolicyAckClaimViews(new List<PolicyAckClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckClaimViewShowsTheStatusInCapitals();
    }
}
