namespace Acme.Workforce.Absence.Tests.Policy.PolicyRollup;

using Acme.Workforce.Absence.Policy.PolicyRollup;

/// <summary>Checks of policy rollup code, run by the project's runner.</summary>
public static class AdaptivePolicyOverrideViewChecks
{
    public static void AdaptivePolicyOverrideViewShowsTheStatusInCapitals()
    {
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("code-898", "name-580");
        var view = AdaptivePolicyOverrideView.ToAdaptivePolicyOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptivePolicyOverrideView.ToAdaptivePolicyOverrideViews(new List<AdaptivePolicyOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptivePolicyOverrideViewShowsTheStatusInCapitals();
    }
}
