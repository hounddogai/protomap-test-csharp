namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RollingBalanceRecordChecks
{
    public static void RollingBalanceRecordStartsInIdle()
    {
        var item = RollingBalanceRecord.NewRollingBalanceRecord("code-730", "label-509");
        Check.Equal(item.Status, RollingBalanceRecordStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingBalanceRecordMovesToReady()
    {
        var item = RollingBalanceRecord.NewRollingBalanceRecord("code-730", "label-509");
        try
        {
            item.MoveTo(RollingBalanceRecordStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingBalanceRecordStatus.Ready, "status");
    }

    public static void RollingBalanceRecordRejectsMovesFromStopped()
    {
        var item = RollingBalanceRecord.NewRollingBalanceRecord("code-730", "label-509");
        item.Status = RollingBalanceRecordStatus.Stopped;
        Check.Fails(() => item.MoveTo(RollingBalanceRecordStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RollingBalanceRecordStartsInIdle();
        RollingBalanceRecordMovesToReady();
        RollingBalanceRecordRejectsMovesFromStopped();
    }
}
