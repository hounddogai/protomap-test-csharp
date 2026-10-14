namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveRateTemplateViewChecks
{
    public static void AdaptiveRateTemplateViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveRateTemplate.NewAdaptiveRateTemplate("code-707", "label-144");
        var view = AdaptiveRateTemplateView.ToAdaptiveRateTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveRateTemplateView.ToAdaptiveRateTemplateViews(new List<AdaptiveRateTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveRateTemplateViewShowsTheStatusInCapitals();
    }
}
