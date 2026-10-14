namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class MeritCycleLedgerChecks
{
    public static void MeritCycleLedgerStartsInProposed()
    {
        var item = MeritCycleLedger.NewMeritCycleLedger("code-167", "label-616");
        Check.Equal(item.Status, MeritCycleLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleLedgerMovesToApproved()
    {
        var item = MeritCycleLedger.NewMeritCycleLedger("code-167", "label-616");
        try
        {
            item.MoveTo(MeritCycleLedgerStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleLedgerStatus.Approved, "status");
    }

    public static void MeritCycleLedgerRejectsMovesFromWithdrawn()
    {
        var item = MeritCycleLedger.NewMeritCycleLedger("code-167", "label-616");
        item.Status = MeritCycleLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MeritCycleLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MeritCycleLedgerStartsInProposed();
        MeritCycleLedgerMovesToApproved();
        MeritCycleLedgerRejectsMovesFromWithdrawn();
    }
}
