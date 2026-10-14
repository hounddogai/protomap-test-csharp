namespace Acme.Workforce.Compliance.Tests.PolicyAck.Draft;

using Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PolicyAckEntryViewChecks
{
    public static void PolicyAckEntryViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckEntry.NewPolicyAckEntry("code-245", "label-807");
        var view = PolicyAckEntryView.ToPolicyAckEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckEntryView.ToPolicyAckEntryViews(new List<PolicyAckEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckEntryViewShowsTheStatusInCapitals();
    }
}
