namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionSlotChecks
{
    public static void RecognitionSlotStartsInDraft()
    {
        var item = RecognitionSlot.NewRecognitionSlot("code-310", "name-761");
        Check.Equal(item.Status, RecognitionSlotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionSlotMovesToSuspended()
    {
        var item = RecognitionSlot.NewRecognitionSlot("code-310", "name-761");
        try
        {
            item.MoveTo(RecognitionSlotStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionSlotStatus.Suspended, "status");
    }

    public static void RecognitionSlotRejectsMovesFromClosed()
    {
        var item = RecognitionSlot.NewRecognitionSlot("code-310", "name-761");
        item.Status = RecognitionSlotStatus.Closed;
        Check.Fails(() => item.MoveTo(RecognitionSlotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RecognitionSlotStartsInDraft();
        RecognitionSlotMovesToSuspended();
        RecognitionSlotRejectsMovesFromClosed();
    }
}
