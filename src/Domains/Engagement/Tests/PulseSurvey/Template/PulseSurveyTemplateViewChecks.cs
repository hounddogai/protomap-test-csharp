namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyTemplateViewChecks
{
    public static void PulseSurveyTemplateViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyTemplate.NewPulseSurveyTemplate("code-358", "label-299");
        var view = PulseSurveyTemplateView.ToPulseSurveyTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyTemplateView.ToPulseSurveyTemplateViews(new List<PulseSurveyTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyTemplateViewShowsTheStatusInCapitals();
    }
}
