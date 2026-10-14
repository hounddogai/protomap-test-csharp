namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountSchedule;

using Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

/// <summary>Checks of headcount schedule code, run by the project's runner.</summary>
public static class HeadcountLedgerChecks
{
    public static void HeadcountLedgerStartsInIdle()
    {
        var item = HeadcountLedger.NewHeadcountLedger("code-656", "title-1");
        Check.Equal(item.Status, HeadcountLedgerStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountLedgerMovesToStopped()
    {
        var item = HeadcountLedger.NewHeadcountLedger("code-656", "title-1");
        try
        {
            item.MoveTo(HeadcountLedgerStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountLedgerStatus.Stopped, "status");
    }

    public static void HeadcountLedgerRejectsMovesFromStopped()
    {
        var item = HeadcountLedger.NewHeadcountLedger("code-656", "title-1");
        item.Status = HeadcountLedgerStatus.Stopped;
        Check.Fails(() => item.MoveTo(HeadcountLedgerStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HeadcountLedgerStartsInIdle();
        HeadcountLedgerMovesToStopped();
        HeadcountLedgerRejectsMovesFromStopped();
    }
}
