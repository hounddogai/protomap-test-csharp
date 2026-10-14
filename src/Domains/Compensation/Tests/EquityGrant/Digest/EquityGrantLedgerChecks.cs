namespace Acme.Workforce.Compensation.Tests.EquityGrant.Digest;

using Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class EquityGrantLedgerChecks
{
    public static void EquityGrantLedgerStartsInRequested()
    {
        var item = EquityGrantLedger.NewEquityGrantLedger("code-125", "title-728");
        Check.Equal(item.Status, EquityGrantLedgerStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantLedgerMovesToRevoked()
    {
        var item = EquityGrantLedger.NewEquityGrantLedger("code-125", "title-728");
        try
        {
            item.MoveTo(EquityGrantLedgerStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantLedgerStatus.Revoked, "status");
    }

    public static void EquityGrantLedgerRejectsMovesFromExpired()
    {
        var item = EquityGrantLedger.NewEquityGrantLedger("code-125", "title-728");
        item.Status = EquityGrantLedgerStatus.Expired;
        Check.Fails(() => item.MoveTo(EquityGrantLedgerStatus.Requested), "move");
    }

    public static void RunAll()
    {
        EquityGrantLedgerStartsInRequested();
        EquityGrantLedgerMovesToRevoked();
        EquityGrantLedgerRejectsMovesFromExpired();
    }
}
