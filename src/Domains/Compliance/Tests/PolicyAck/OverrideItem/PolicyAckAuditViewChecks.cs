namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckAuditViewChecks
{
    public static void PolicyAckAuditViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckAudit.NewPolicyAckAudit("code-356", "label-410");
        var view = PolicyAckAuditView.ToPolicyAckAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckAuditView.ToPolicyAckAuditViews(new List<PolicyAckAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckAuditViewShowsTheStatusInCapitals();
    }
}
