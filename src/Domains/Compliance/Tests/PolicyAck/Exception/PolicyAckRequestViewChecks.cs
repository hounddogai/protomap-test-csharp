namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckRequestViewChecks
{
    public static void PolicyAckRequestViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckRequest.NewPolicyAckRequest("code-606", "title-716");
        var view = PolicyAckRequestView.ToPolicyAckRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckRequestView.ToPolicyAckRequestViews(new List<PolicyAckRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckRequestViewShowsTheStatusInCapitals();
    }
}
