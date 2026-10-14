namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RecognitionSnapshotViewChecks
{
    public static void RecognitionSnapshotViewShowsTheStatusInCapitals()
    {
        var item = RecognitionSnapshot.NewRecognitionSnapshot("code-380", "label-895");
        var view = RecognitionSnapshotView.ToRecognitionSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionSnapshotView.ToRecognitionSnapshotViews(new List<RecognitionSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionSnapshotViewShowsTheStatusInCapitals();
    }
}
