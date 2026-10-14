namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RecognitionCycleChecks
{
    public static void RecognitionCycleStartsInDraft()
    {
        var item = RecognitionCycle.NewRecognitionCycle("code-384", "name-558");
        Check.Equal(item.Status, RecognitionCycleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionCycleMovesToPending()
    {
        var item = RecognitionCycle.NewRecognitionCycle("code-384", "name-558");
        try
        {
            item.MoveTo(RecognitionCycleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionCycleStatus.Pending, "status");
    }

    public static void RecognitionCycleRejectsMovesFromClosed()
    {
        var item = RecognitionCycle.NewRecognitionCycle("code-384", "name-558");
        item.Status = RecognitionCycleStatus.Closed;
        Check.Fails(() => item.MoveTo(RecognitionCycleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RecognitionCycleStartsInDraft();
        RecognitionCycleMovesToPending();
        RecognitionCycleRejectsMovesFromClosed();
    }
}
