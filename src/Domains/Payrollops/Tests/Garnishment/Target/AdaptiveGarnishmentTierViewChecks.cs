namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentTierViewChecks
{
    public static void AdaptiveGarnishmentTierViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveGarnishmentTier.NewAdaptiveGarnishmentTier("code-833", "name-594");
        var view = AdaptiveGarnishmentTierView.ToAdaptiveGarnishmentTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveGarnishmentTierView.ToAdaptiveGarnishmentTierViews(new List<AdaptiveGarnishmentTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentTierViewShowsTheStatusInCapitals();
    }
}
