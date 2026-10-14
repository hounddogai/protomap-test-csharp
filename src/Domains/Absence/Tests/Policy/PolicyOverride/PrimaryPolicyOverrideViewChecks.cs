namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class PrimaryPolicyOverrideViewChecks
{
    public static void PrimaryPolicyOverrideViewShowsTheStatusInCapitals()
    {
        var item = PrimaryPolicyOverride.NewPrimaryPolicyOverride("code-589", "name-224");
        var view = PrimaryPolicyOverrideView.ToPrimaryPolicyOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryPolicyOverrideView.ToPrimaryPolicyOverrideViews(new List<PrimaryPolicyOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryPolicyOverrideViewShowsTheStatusInCapitals();
    }
}
