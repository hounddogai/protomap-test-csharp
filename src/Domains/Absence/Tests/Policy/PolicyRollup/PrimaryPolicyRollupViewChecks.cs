namespace Acme.Workforce.Absence.Tests.Policy.PolicyRollup;

using Acme.Workforce.Absence.Policy.PolicyRollup;

/// <summary>Checks of policy rollup code, run by the project's runner.</summary>
public static class PrimaryPolicyRollupViewChecks
{
    public static void PrimaryPolicyRollupViewShowsTheStatusInCapitals()
    {
        var item = PrimaryPolicyRollup.NewPrimaryPolicyRollup("code-999", "label-331");
        var view = PrimaryPolicyRollupView.ToPrimaryPolicyRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryPolicyRollupView.ToPrimaryPolicyRollupViews(new List<PrimaryPolicyRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryPolicyRollupViewShowsTheStatusInCapitals();
    }
}
