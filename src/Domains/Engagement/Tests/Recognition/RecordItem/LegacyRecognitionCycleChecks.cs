namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyRecognitionCycleChecks
{
    public static void LegacyRecognitionCycleStartsInIdle()
    {
        var item = LegacyRecognitionCycle.NewLegacyRecognitionCycle("code-545", "title-615");
        Check.Equal(item.Status, LegacyRecognitionCycleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyRecognitionCycleMovesToReady()
    {
        var item = LegacyRecognitionCycle.NewLegacyRecognitionCycle("code-545", "title-615");
        try
        {
            item.MoveTo(LegacyRecognitionCycleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyRecognitionCycleStatus.Ready, "status");
    }

    public static void LegacyRecognitionCycleRejectsMovesFromStopped()
    {
        var item = LegacyRecognitionCycle.NewLegacyRecognitionCycle("code-545", "title-615");
        item.Status = LegacyRecognitionCycleStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyRecognitionCycleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyRecognitionCycleStartsInIdle();
        LegacyRecognitionCycleMovesToReady();
        LegacyRecognitionCycleRejectsMovesFromStopped();
    }
}
