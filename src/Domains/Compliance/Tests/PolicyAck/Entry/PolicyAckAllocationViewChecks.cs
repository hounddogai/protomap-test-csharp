namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PolicyAckAllocationViewChecks
{
    public static void PolicyAckAllocationViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckAllocation.NewPolicyAckAllocation("code-974", "label-131");
        var view = PolicyAckAllocationView.ToPolicyAckAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckAllocationView.ToPolicyAckAllocationViews(new List<PolicyAckAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckAllocationViewShowsTheStatusInCapitals();
    }
}
