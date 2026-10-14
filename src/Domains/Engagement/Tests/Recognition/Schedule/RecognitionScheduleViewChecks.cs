namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RecognitionScheduleViewChecks
{
    public static void RecognitionScheduleViewShowsTheStatusInCapitals()
    {
        var item = RecognitionSchedule.NewRecognitionSchedule("code-878", "title-180");
        var view = RecognitionScheduleView.ToRecognitionScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionScheduleView.ToRecognitionScheduleViews(new List<RecognitionSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionScheduleViewShowsTheStatusInCapitals();
    }
}
