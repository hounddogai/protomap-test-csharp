namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackPulseSurveySegmentViewChecks
{
    public static void FallbackPulseSurveySegmentViewShowsTheStatusInCapitals()
    {
        var item = FallbackPulseSurveySegment.NewFallbackPulseSurveySegment("code-226", "name-42");
        var view = FallbackPulseSurveySegmentView.ToFallbackPulseSurveySegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPulseSurveySegmentView.ToFallbackPulseSurveySegmentViews(new List<FallbackPulseSurveySegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPulseSurveySegmentViewShowsTheStatusInCapitals();
    }
}
