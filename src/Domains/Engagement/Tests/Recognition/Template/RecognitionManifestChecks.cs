namespace Acme.Workforce.Engagement.Tests.Recognition.Template;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RecognitionManifestChecks
{
    public static void RecognitionManifestStartsInRequested()
    {
        var item = RecognitionManifest.NewRecognitionManifest("code-762", "label-215");
        Check.Equal(item.Status, RecognitionManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionManifestMovesToRevoked()
    {
        var item = RecognitionManifest.NewRecognitionManifest("code-762", "label-215");
        try
        {
            item.MoveTo(RecognitionManifestStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionManifestStatus.Revoked, "status");
    }

    public static void RecognitionManifestRejectsMovesFromExpired()
    {
        var item = RecognitionManifest.NewRecognitionManifest("code-762", "label-215");
        item.Status = RecognitionManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(RecognitionManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RecognitionManifestStartsInRequested();
        RecognitionManifestMovesToRevoked();
        RecognitionManifestRejectsMovesFromExpired();
    }
}
