namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantLedger;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;

/// <summary>Checks of grant ledger code, run by the project's runner.</summary>
public static class ScheduledEquityGrantLedgerChecks
{
    public static void ScheduledEquityGrantLedgerStartsInProposed()
    {
        var item = ScheduledEquityGrantLedger.NewScheduledEquityGrantLedger("code-853", "label-164");
        Check.Equal(item.Status, ScheduledEquityGrantLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledEquityGrantLedgerMovesToWithdrawn()
    {
        var item = ScheduledEquityGrantLedger.NewScheduledEquityGrantLedger("code-853", "label-164");
        try
        {
            item.MoveTo(ScheduledEquityGrantLedgerStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledEquityGrantLedgerStatus.Withdrawn, "status");
    }

    public static void ScheduledEquityGrantLedgerRejectsMovesFromWithdrawn()
    {
        var item = ScheduledEquityGrantLedger.NewScheduledEquityGrantLedger("code-853", "label-164");
        item.Status = ScheduledEquityGrantLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledEquityGrantLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledEquityGrantLedgerStartsInProposed();
        ScheduledEquityGrantLedgerMovesToWithdrawn();
        ScheduledEquityGrantLedgerRejectsMovesFromWithdrawn();
    }
}
