namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionSlotViewChecks
{
    public static void RecognitionSlotViewShowsTheStatusInCapitals()
    {
        var item = RecognitionSlot.NewRecognitionSlot("code-310", "name-761");
        var view = RecognitionSlotView.ToRecognitionSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionSlotView.ToRecognitionSlotViews(new List<RecognitionSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionSlotViewShowsTheStatusInCapitals();
    }
}
