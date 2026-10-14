namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyNoticeViewChecks
{
    public static void PulseSurveyNoticeViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyNotice.NewPulseSurveyNotice("code-202", "title-160");
        var view = PulseSurveyNoticeView.ToPulseSurveyNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyNoticeView.ToPulseSurveyNoticeViews(new List<PulseSurveyNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyNoticeViewShowsTheStatusInCapitals();
    }
}
