namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanLedgerChecks
{
    public static void ReturnPlanLedgerStartsInProposed()
    {
        var item = ReturnPlanLedger.NewReturnPlanLedger("code-792", "title-100");
        Check.Equal(item.Status, ReturnPlanLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanLedgerMovesToApproved()
    {
        var item = ReturnPlanLedger.NewReturnPlanLedger("code-792", "title-100");
        try
        {
            item.MoveTo(ReturnPlanLedgerStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanLedgerStatus.Approved, "status");
    }

    public static void ReturnPlanLedgerRejectsMovesFromWithdrawn()
    {
        var item = ReturnPlanLedger.NewReturnPlanLedger("code-792", "title-100");
        item.Status = ReturnPlanLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ReturnPlanLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ReturnPlanLedgerStartsInProposed();
        ReturnPlanLedgerMovesToApproved();
        ReturnPlanLedgerRejectsMovesFromWithdrawn();
    }
}
