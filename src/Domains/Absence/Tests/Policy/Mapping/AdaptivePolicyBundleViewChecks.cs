namespace Acme.Workforce.Absence.Tests.Policy.Mapping;

using Acme.Workforce.Absence.Policy.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptivePolicyBundleViewChecks
{
    public static void AdaptivePolicyBundleViewShowsTheStatusInCapitals()
    {
        var item = AdaptivePolicyBundle.NewAdaptivePolicyBundle("code-738", "label-214");
        var view = AdaptivePolicyBundleView.ToAdaptivePolicyBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptivePolicyBundleView.ToAdaptivePolicyBundleViews(new List<AdaptivePolicyBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptivePolicyBundleViewShowsTheStatusInCapitals();
    }
}
