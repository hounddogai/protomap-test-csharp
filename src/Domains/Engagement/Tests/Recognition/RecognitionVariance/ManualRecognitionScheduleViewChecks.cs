namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class ManualRecognitionScheduleViewChecks
{
    public static void ManualRecognitionScheduleViewShowsTheStatusInCapitals()
    {
        var item = ManualRecognitionSchedule.NewManualRecognitionSchedule("code-325", "title-418");
        var view = ManualRecognitionScheduleView.ToManualRecognitionScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualRecognitionScheduleView.ToManualRecognitionScheduleViews(new List<ManualRecognitionSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualRecognitionScheduleViewShowsTheStatusInCapitals();
    }
}
