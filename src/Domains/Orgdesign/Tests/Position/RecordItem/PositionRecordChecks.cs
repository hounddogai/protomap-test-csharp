namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionRecordChecks
{
    public static void PositionRecordStartsInIdle()
    {
        var item = PositionRecord.NewPositionRecord("code-490", "name-855");
        Check.Equal(item.Status, PositionRecordStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionRecordMovesToReady()
    {
        var item = PositionRecord.NewPositionRecord("code-490", "name-855");
        try
        {
            item.MoveTo(PositionRecordStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionRecordStatus.Ready, "status");
    }

    public static void PositionRecordRejectsMovesFromStopped()
    {
        var item = PositionRecord.NewPositionRecord("code-490", "name-855");
        item.Status = PositionRecordStatus.Stopped;
        Check.Fails(() => item.MoveTo(PositionRecordStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PositionRecordStartsInIdle();
        PositionRecordMovesToReady();
        PositionRecordRejectsMovesFromStopped();
    }
}
