namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RecognitionProfileViewChecks
{
    public static void RecognitionProfileViewShowsTheStatusInCapitals()
    {
        var item = RecognitionProfile.NewRecognitionProfile("code-771", "label-29");
        var view = RecognitionProfileView.ToRecognitionProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionProfileView.ToRecognitionProfileViews(new List<RecognitionProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionProfileViewShowsTheStatusInCapitals();
    }
}
