namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionOverrideViewChecks
{
    public static void RecognitionOverrideViewShowsTheStatusInCapitals()
    {
        var item = RecognitionOverride.NewRecognitionOverride("code-725", "label-571");
        var view = RecognitionOverrideView.ToRecognitionOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionOverrideView.ToRecognitionOverrideViews(new List<RecognitionOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionOverrideViewShowsTheStatusInCapitals();
    }
}
