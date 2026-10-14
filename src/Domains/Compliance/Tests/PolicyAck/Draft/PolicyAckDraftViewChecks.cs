namespace Acme.Workforce.Compliance.Tests.PolicyAck.Draft;

using Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PolicyAckDraftViewChecks
{
    public static void PolicyAckDraftViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckDraft.NewPolicyAckDraft("code-496", "title-705");
        var view = PolicyAckDraftView.ToPolicyAckDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckDraftView.ToPolicyAckDraftViews(new List<PolicyAckDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckDraftViewShowsTheStatusInCapitals();
    }
}
