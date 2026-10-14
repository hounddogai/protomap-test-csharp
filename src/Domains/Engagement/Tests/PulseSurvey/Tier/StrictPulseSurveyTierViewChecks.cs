namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Tier;

using Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictPulseSurveyTierViewChecks
{
    public static void StrictPulseSurveyTierViewShowsTheStatusInCapitals()
    {
        var item = StrictPulseSurveyTier.NewStrictPulseSurveyTier("code-506", "label-131");
        var view = StrictPulseSurveyTierView.ToStrictPulseSurveyTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPulseSurveyTierView.ToStrictPulseSurveyTierViews(new List<StrictPulseSurveyTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPulseSurveyTierViewShowsTheStatusInCapitals();
    }
}
