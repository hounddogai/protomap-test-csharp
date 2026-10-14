namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyMappingViewChecks
{
    public static void PulseSurveyMappingViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyMapping.NewPulseSurveyMapping("code-976", "label-253");
        var view = PulseSurveyMappingView.ToPulseSurveyMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyMappingView.ToPulseSurveyMappingViews(new List<PulseSurveyMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyMappingViewShowsTheStatusInCapitals();
    }
}
