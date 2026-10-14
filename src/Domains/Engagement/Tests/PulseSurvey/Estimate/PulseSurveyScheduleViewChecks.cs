namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyScheduleViewChecks
{
    public static void PulseSurveyScheduleViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveySchedule.NewPulseSurveySchedule("code-440", "title-558");
        var view = PulseSurveyScheduleView.ToPulseSurveyScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyScheduleView.ToPulseSurveyScheduleViews(new List<PulseSurveySchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyScheduleViewShowsTheStatusInCapitals();
    }
}
