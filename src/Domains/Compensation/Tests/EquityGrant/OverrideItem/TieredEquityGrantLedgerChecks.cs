namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredEquityGrantLedgerChecks
{
    public static void TieredEquityGrantLedgerStartsInDraft()
    {
        var item = TieredEquityGrantLedger.NewTieredEquityGrantLedger("code-120", "name-257");
        Check.Equal(item.Status, TieredEquityGrantLedgerStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredEquityGrantLedgerMovesToActive()
    {
        var item = TieredEquityGrantLedger.NewTieredEquityGrantLedger("code-120", "name-257");
        try
        {
            item.MoveTo(TieredEquityGrantLedgerStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredEquityGrantLedgerStatus.Active, "status");
    }

    public static void TieredEquityGrantLedgerRejectsMovesFromClosed()
    {
        var item = TieredEquityGrantLedger.NewTieredEquityGrantLedger("code-120", "name-257");
        item.Status = TieredEquityGrantLedgerStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredEquityGrantLedgerStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredEquityGrantLedgerStartsInDraft();
        TieredEquityGrantLedgerMovesToActive();
        TieredEquityGrantLedgerRejectsMovesFromClosed();
    }
}
