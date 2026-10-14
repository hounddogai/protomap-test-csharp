namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionRequestChecks
{
    public static void RecognitionRequestStartsInIdle()
    {
        var item = RecognitionRequest.NewRecognitionRequest("code-450", "title-285");
        Check.Equal(item.Status, RecognitionRequestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionRequestMovesToStopped()
    {
        var item = RecognitionRequest.NewRecognitionRequest("code-450", "title-285");
        try
        {
            item.MoveTo(RecognitionRequestStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionRequestStatus.Stopped, "status");
    }

    public static void RecognitionRequestRejectsMovesFromStopped()
    {
        var item = RecognitionRequest.NewRecognitionRequest("code-450", "title-285");
        item.Status = RecognitionRequestStatus.Stopped;
        Check.Fails(() => item.MoveTo(RecognitionRequestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RecognitionRequestStartsInIdle();
        RecognitionRequestMovesToStopped();
        RecognitionRequestRejectsMovesFromStopped();
    }
}
