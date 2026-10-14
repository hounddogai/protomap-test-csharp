namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class FallbackRecognitionDigestViewChecks
{
    public static void FallbackRecognitionDigestViewShowsTheStatusInCapitals()
    {
        var item = FallbackRecognitionDigest.NewFallbackRecognitionDigest("code-721", "label-133");
        var view = FallbackRecognitionDigestView.ToFallbackRecognitionDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackRecognitionDigestView.ToFallbackRecognitionDigestViews(new List<FallbackRecognitionDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackRecognitionDigestViewShowsTheStatusInCapitals();
    }
}
