namespace Acme.Workforce.Absence.Tests.Balance.Segment;

using Acme.Workforce.Absence.Balance.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BalanceRecordChecks
{
    public static void BalanceRecordStartsInIdle()
    {
        var item = BalanceRecord.NewBalanceRecord("code-341", "name-114");
        Check.Equal(item.Status, BalanceRecordStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceRecordMovesToWarming()
    {
        var item = BalanceRecord.NewBalanceRecord("code-341", "name-114");
        try
        {
            item.MoveTo(BalanceRecordStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceRecordStatus.Warming, "status");
    }

    public static void BalanceRecordRejectsMovesFromStopped()
    {
        var item = BalanceRecord.NewBalanceRecord("code-341", "name-114");
        item.Status = BalanceRecordStatus.Stopped;
        Check.Fails(() => item.MoveTo(BalanceRecordStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BalanceRecordStartsInIdle();
        BalanceRecordMovesToWarming();
        BalanceRecordRejectsMovesFromStopped();
    }
}
