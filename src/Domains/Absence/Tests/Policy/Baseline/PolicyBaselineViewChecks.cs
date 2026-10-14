namespace Acme.Workforce.Absence.Tests.Policy.Baseline;

using Acme.Workforce.Absence.Policy.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PolicyBaselineViewChecks
{
    public static void PolicyBaselineViewShowsTheStatusInCapitals()
    {
        var item = PolicyBaseline.NewPolicyBaseline("code-772", "name-559");
        var view = PolicyBaselineView.ToPolicyBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyBaselineView.ToPolicyBaselineViews(new List<PolicyBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyBaselineViewShowsTheStatusInCapitals();
    }
}
