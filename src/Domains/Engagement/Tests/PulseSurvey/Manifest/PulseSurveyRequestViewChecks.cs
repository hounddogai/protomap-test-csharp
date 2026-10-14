namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PulseSurveyRequestViewChecks
{
    public static void PulseSurveyRequestViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyRequest.NewPulseSurveyRequest("code-100", "title-13");
        var view = PulseSurveyRequestView.ToPulseSurveyRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyRequestView.ToPulseSurveyRequestViews(new List<PulseSurveyRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyRequestViewShowsTheStatusInCapitals();
    }
}
