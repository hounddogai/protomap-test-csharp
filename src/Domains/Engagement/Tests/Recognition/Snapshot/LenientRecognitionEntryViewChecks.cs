namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LenientRecognitionEntryViewChecks
{
    public static void LenientRecognitionEntryViewShowsTheStatusInCapitals()
    {
        var item = LenientRecognitionEntry.NewLenientRecognitionEntry("code-814", "title-357");
        var view = LenientRecognitionEntryView.ToLenientRecognitionEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientRecognitionEntryView.ToLenientRecognitionEntryViews(new List<LenientRecognitionEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientRecognitionEntryViewShowsTheStatusInCapitals();
    }
}
