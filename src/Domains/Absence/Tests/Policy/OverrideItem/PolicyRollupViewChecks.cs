namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyRollupViewChecks
{
    public static void PolicyRollupViewShowsTheStatusInCapitals()
    {
        var item = PolicyRollup.NewPolicyRollup("code-777", "title-514");
        var view = PolicyRollupView.ToPolicyRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyRollupView.ToPolicyRollupViews(new List<PolicyRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyRollupViewShowsTheStatusInCapitals();
    }
}
