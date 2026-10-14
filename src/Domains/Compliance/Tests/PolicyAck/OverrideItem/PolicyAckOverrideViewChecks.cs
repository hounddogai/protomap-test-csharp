namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckOverrideViewChecks
{
    public static void PolicyAckOverrideViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckOverride.NewPolicyAckOverride("code-575", "label-308");
        var view = PolicyAckOverrideView.ToPolicyAckOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckOverrideView.ToPolicyAckOverrideViews(new List<PolicyAckOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckOverrideViewShowsTheStatusInCapitals();
    }
}
