namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SeasonalPolicyAckRequestViewChecks
{
    public static void SeasonalPolicyAckRequestViewShowsTheStatusInCapitals()
    {
        var item = SeasonalPolicyAckRequest.NewSeasonalPolicyAckRequest("code-968", "label-120");
        var view = SeasonalPolicyAckRequestView.ToSeasonalPolicyAckRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalPolicyAckRequestView.ToSeasonalPolicyAckRequestViews(new List<SeasonalPolicyAckRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalPolicyAckRequestViewShowsTheStatusInCapitals();
    }
}
