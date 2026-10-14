namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RecognitionProfileChecks
{
    public static void RecognitionProfileStartsInRequested()
    {
        var item = RecognitionProfile.NewRecognitionProfile("code-771", "label-29");
        Check.Equal(item.Status, RecognitionProfileStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionProfileMovesToGranted()
    {
        var item = RecognitionProfile.NewRecognitionProfile("code-771", "label-29");
        try
        {
            item.MoveTo(RecognitionProfileStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionProfileStatus.Granted, "status");
    }

    public static void RecognitionProfileRejectsMovesFromExpired()
    {
        var item = RecognitionProfile.NewRecognitionProfile("code-771", "label-29");
        item.Status = RecognitionProfileStatus.Expired;
        Check.Fails(() => item.MoveTo(RecognitionProfileStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RecognitionProfileStartsInRequested();
        RecognitionProfileMovesToGranted();
        RecognitionProfileRejectsMovesFromExpired();
    }
}
