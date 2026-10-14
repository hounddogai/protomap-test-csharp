namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientEquityGrantLedgerChecks
{
    public static void LenientEquityGrantLedgerStartsInOpen()
    {
        var item = LenientEquityGrantLedger.NewLenientEquityGrantLedger("code-416", "title-25");
        Check.Equal(item.Status, LenientEquityGrantLedgerStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientEquityGrantLedgerMovesToReopened()
    {
        var item = LenientEquityGrantLedger.NewLenientEquityGrantLedger("code-416", "title-25");
        try
        {
            item.MoveTo(LenientEquityGrantLedgerStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientEquityGrantLedgerStatus.Reopened, "status");
    }

    public static void LenientEquityGrantLedgerRejectsMovesFromArchived()
    {
        var item = LenientEquityGrantLedger.NewLenientEquityGrantLedger("code-416", "title-25");
        item.Status = LenientEquityGrantLedgerStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientEquityGrantLedgerStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientEquityGrantLedgerStartsInOpen();
        LenientEquityGrantLedgerMovesToReopened();
        LenientEquityGrantLedgerRejectsMovesFromArchived();
    }
}
