namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyBundleViewChecks
{
    public static void PolicyBundleViewShowsTheStatusInCapitals()
    {
        var item = PolicyBundle.NewPolicyBundle("code-832", "label-514");
        var view = PolicyBundleView.ToPolicyBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyBundleView.ToPolicyBundleViews(new List<PolicyBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyBundleViewShowsTheStatusInCapitals();
    }
}
