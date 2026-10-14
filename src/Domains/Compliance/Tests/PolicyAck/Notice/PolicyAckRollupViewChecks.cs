namespace Acme.Workforce.Compliance.Tests.PolicyAck.Notice;

using Acme.Workforce.Compliance.PolicyAck.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyAckRollupViewChecks
{
    public static void PolicyAckRollupViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckRollup.NewPolicyAckRollup("code-201", "name-370");
        var view = PolicyAckRollupView.ToPolicyAckRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckRollupView.ToPolicyAckRollupViews(new List<PolicyAckRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckRollupViewShowsTheStatusInCapitals();
    }
}
