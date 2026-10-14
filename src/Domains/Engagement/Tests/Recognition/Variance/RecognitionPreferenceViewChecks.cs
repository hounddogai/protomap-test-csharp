namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionPreferenceViewChecks
{
    public static void RecognitionPreferenceViewShowsTheStatusInCapitals()
    {
        var item = RecognitionPreference.NewRecognitionPreference("code-961", "label-235");
        var view = RecognitionPreferenceView.ToRecognitionPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionPreferenceView.ToRecognitionPreferenceViews(new List<RecognitionPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionPreferenceViewShowsTheStatusInCapitals();
    }
}
