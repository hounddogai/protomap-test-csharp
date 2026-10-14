namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DeskRecordChecks
{
    public static void DeskRecordStartsInIdle()
    {
        var item = DeskRecord.NewDeskRecord("code-781", "title-197");
        Check.Equal(item.Status, DeskRecordStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskRecordMovesToStopped()
    {
        var item = DeskRecord.NewDeskRecord("code-781", "title-197");
        try
        {
            item.MoveTo(DeskRecordStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskRecordStatus.Stopped, "status");
    }

    public static void DeskRecordRejectsMovesFromStopped()
    {
        var item = DeskRecord.NewDeskRecord("code-781", "title-197");
        item.Status = DeskRecordStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeskRecordStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeskRecordStartsInIdle();
        DeskRecordMovesToStopped();
        DeskRecordRejectsMovesFromStopped();
    }
}
