namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class StrictRecognitionCycleChecks
{
    public static void StrictRecognitionCycleStartsInPlanned()
    {
        var item = StrictRecognitionCycle.NewStrictRecognitionCycle("code-942", "name-212");
        Check.Equal(item.Status, StrictRecognitionCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictRecognitionCycleMovesToCancelled()
    {
        var item = StrictRecognitionCycle.NewStrictRecognitionCycle("code-942", "name-212");
        try
        {
            item.MoveTo(StrictRecognitionCycleStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictRecognitionCycleStatus.Cancelled, "status");
    }

    public static void StrictRecognitionCycleRejectsMovesFromCancelled()
    {
        var item = StrictRecognitionCycle.NewStrictRecognitionCycle("code-942", "name-212");
        item.Status = StrictRecognitionCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictRecognitionCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictRecognitionCycleStartsInPlanned();
        StrictRecognitionCycleMovesToCancelled();
        StrictRecognitionCycleRejectsMovesFromCancelled();
    }
}
