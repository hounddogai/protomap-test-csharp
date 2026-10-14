namespace Acme.Workforce.Engagement.Tests.PulseSurvey.RecordItem;

using Acme.Workforce.Engagement.PulseSurvey.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalPulseSurveyRecordViewChecks
{
    public static void GlobalPulseSurveyRecordViewShowsTheStatusInCapitals()
    {
        var item = GlobalPulseSurveyRecord.NewGlobalPulseSurveyRecord("code-931", "name-823");
        var view = GlobalPulseSurveyRecordView.ToGlobalPulseSurveyRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPulseSurveyRecordView.ToGlobalPulseSurveyRecordViews(new List<GlobalPulseSurveyRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPulseSurveyRecordViewShowsTheStatusInCapitals();
    }
}
