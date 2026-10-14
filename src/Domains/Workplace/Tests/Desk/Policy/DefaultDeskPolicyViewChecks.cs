namespace Acme.Workforce.Workplace.Tests.Desk.Policy;

using Acme.Workforce.Workplace.Desk.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DefaultDeskPolicyViewChecks
{
    public static void DefaultDeskPolicyViewShowsTheStatusInCapitals()
    {
        var item = DefaultDeskPolicy.NewDefaultDeskPolicy("code-559", "title-308");
        var view = DefaultDeskPolicyView.ToDefaultDeskPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultDeskPolicyView.ToDefaultDeskPolicyViews(new List<DefaultDeskPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultDeskPolicyViewShowsTheStatusInCapitals();
    }
}
