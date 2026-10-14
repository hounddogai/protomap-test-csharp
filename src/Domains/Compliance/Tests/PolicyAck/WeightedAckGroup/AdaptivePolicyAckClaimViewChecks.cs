namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class AdaptivePolicyAckClaimViewChecks
{
    public static void AdaptivePolicyAckClaimViewShowsTheStatusInCapitals()
    {
        var item = AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim("code-581", "label-804");
        var view = AdaptivePolicyAckClaimView.ToAdaptivePolicyAckClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptivePolicyAckClaimView.ToAdaptivePolicyAckClaimViews(new List<AdaptivePolicyAckClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptivePolicyAckClaimViewShowsTheStatusInCapitals();
    }
}
