namespace Acme.Workforce.Compliance.Tests.PolicyAck.Milestone;

using Acme.Workforce.Compliance.PolicyAck.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PolicyAckWindowViewChecks
{
    public static void PolicyAckWindowViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckWindow.NewPolicyAckWindow("code-202", "name-110");
        var view = PolicyAckWindowView.ToPolicyAckWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckWindowView.ToPolicyAckWindowViews(new List<PolicyAckWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckWindowViewShowsTheStatusInCapitals();
    }
}
