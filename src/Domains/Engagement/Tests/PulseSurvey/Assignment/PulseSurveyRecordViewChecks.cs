namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyRecordViewChecks
{
    public static void PulseSurveyRecordViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyRecord.NewPulseSurveyRecord("code-936", "name-423");
        var view = PulseSurveyRecordView.ToPulseSurveyRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyRecordView.ToPulseSurveyRecordViews(new List<PulseSurveyRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyRecordViewShowsTheStatusInCapitals();
    }
}
