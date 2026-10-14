namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Exception;

using Acme.Workforce.Engagement.PulseSurvey.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PulseSurveyExceptionViewChecks
{
    public static void PulseSurveyExceptionViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyException.NewPulseSurveyException("code-797", "title-704");
        var view = PulseSurveyExceptionView.ToPulseSurveyExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyExceptionView.ToPulseSurveyExceptionViews(new List<PulseSurveyException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyExceptionViewShowsTheStatusInCapitals();
    }
}
