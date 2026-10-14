namespace Acme.Workforce.Engagement.Tests.Recognition.Window;

using Acme.Workforce.Engagement.Recognition.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RecognitionWindowViewChecks
{
    public static void RecognitionWindowViewShowsTheStatusInCapitals()
    {
        var item = RecognitionWindow.NewRecognitionWindow("code-456", "label-176");
        var view = RecognitionWindowView.ToRecognitionWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionWindowView.ToRecognitionWindowViews(new List<RecognitionWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionWindowViewShowsTheStatusInCapitals();
    }
}
