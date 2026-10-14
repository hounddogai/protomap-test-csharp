namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class FallbackRecognitionDigestChecks
{
    public static void FallbackRecognitionDigestStartsInIdle()
    {
        var item = FallbackRecognitionDigest.NewFallbackRecognitionDigest("code-721", "label-133");
        Check.Equal(item.Status, FallbackRecognitionDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackRecognitionDigestMovesToWarming()
    {
        var item = FallbackRecognitionDigest.NewFallbackRecognitionDigest("code-721", "label-133");
        try
        {
            item.MoveTo(FallbackRecognitionDigestStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackRecognitionDigestStatus.Warming, "status");
    }

    public static void FallbackRecognitionDigestRejectsMovesFromStopped()
    {
        var item = FallbackRecognitionDigest.NewFallbackRecognitionDigest("code-721", "label-133");
        item.Status = FallbackRecognitionDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackRecognitionDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackRecognitionDigestStartsInIdle();
        FallbackRecognitionDigestMovesToWarming();
        FallbackRecognitionDigestRejectsMovesFromStopped();
    }
}
