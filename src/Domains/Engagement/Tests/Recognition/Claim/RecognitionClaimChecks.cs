namespace Acme.Workforce.Engagement.Tests.Recognition.Claim;

using Acme.Workforce.Engagement.Recognition.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RecognitionClaimChecks
{
    public static void RecognitionClaimStartsInIdle()
    {
        var item = RecognitionClaim.NewRecognitionClaim("code-755", "name-368");
        Check.Equal(item.Status, RecognitionClaimStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionClaimMovesToReady()
    {
        var item = RecognitionClaim.NewRecognitionClaim("code-755", "name-368");
        try
        {
            item.MoveTo(RecognitionClaimStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionClaimStatus.Ready, "status");
    }

    public static void RecognitionClaimRejectsMovesFromStopped()
    {
        var item = RecognitionClaim.NewRecognitionClaim("code-755", "name-368");
        item.Status = RecognitionClaimStatus.Stopped;
        Check.Fails(() => item.MoveTo(RecognitionClaimStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RecognitionClaimStartsInIdle();
        RecognitionClaimMovesToReady();
        RecognitionClaimRejectsMovesFromStopped();
    }
}
