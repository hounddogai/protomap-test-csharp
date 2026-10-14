namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class ManualPulseSurveyTierViewChecks
{
    public static void ManualPulseSurveyTierViewShowsTheStatusInCapitals()
    {
        var item = ManualPulseSurveyTier.NewManualPulseSurveyTier("code-445", "label-314");
        var view = ManualPulseSurveyTierView.ToManualPulseSurveyTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualPulseSurveyTierView.ToManualPulseSurveyTierViews(new List<ManualPulseSurveyTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualPulseSurveyTierViewShowsTheStatusInCapitals();
    }
}
