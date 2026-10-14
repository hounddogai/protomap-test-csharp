namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PolicyAckBaselineViewChecks
{
    public static void PolicyAckBaselineViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckBaseline.NewPolicyAckBaseline("code-343", "title-148");
        var view = PolicyAckBaselineView.ToPolicyAckBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckBaselineView.ToPolicyAckBaselineViews(new List<PolicyAckBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckBaselineViewShowsTheStatusInCapitals();
    }
}
