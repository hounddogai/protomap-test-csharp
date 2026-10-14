namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class RollingPolicyOverrideViewChecks
{
    public static void RollingPolicyOverrideViewShowsTheStatusInCapitals()
    {
        var item = RollingPolicyOverride.NewRollingPolicyOverride("code-598", "name-579");
        var view = RollingPolicyOverrideView.ToRollingPolicyOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingPolicyOverrideView.ToRollingPolicyOverrideViews(new List<RollingPolicyOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingPolicyOverrideViewShowsTheStatusInCapitals();
    }
}
