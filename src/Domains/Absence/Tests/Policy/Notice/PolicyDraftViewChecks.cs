namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyDraftViewChecks
{
    public static void PolicyDraftViewShowsTheStatusInCapitals()
    {
        var item = PolicyDraft.NewPolicyDraft("code-367", "label-836");
        var view = PolicyDraftView.ToPolicyDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyDraftView.ToPolicyDraftViews(new List<PolicyDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyDraftViewShowsTheStatusInCapitals();
    }
}
