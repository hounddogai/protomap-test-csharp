namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class AdaptivePayBandGroupViewChecks
{
    public static void AdaptivePayBandGroupViewShowsTheStatusInCapitals()
    {
        var item = AdaptivePayBandGroup.NewAdaptivePayBandGroup("code-876", "title-841");
        var view = AdaptivePayBandGroupView.ToAdaptivePayBandGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptivePayBandGroupView.ToAdaptivePayBandGroupViews(new List<AdaptivePayBandGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptivePayBandGroupViewShowsTheStatusInCapitals();
    }
}
