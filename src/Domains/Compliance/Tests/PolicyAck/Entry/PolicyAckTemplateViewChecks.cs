namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PolicyAckTemplateViewChecks
{
    public static void PolicyAckTemplateViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckTemplate.NewPolicyAckTemplate("code-718", "name-307");
        var view = PolicyAckTemplateView.ToPolicyAckTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckTemplateView.ToPolicyAckTemplateViews(new List<PolicyAckTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckTemplateViewShowsTheStatusInCapitals();
    }
}
