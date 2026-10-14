namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ActionPlanLedgerChecks
{
    public static void ActionPlanLedgerStartsInProposed()
    {
        var item = ActionPlanLedger.NewActionPlanLedger("code-982", "title-662");
        Check.Equal(item.Status, ActionPlanLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanLedgerMovesToRejected()
    {
        var item = ActionPlanLedger.NewActionPlanLedger("code-982", "title-662");
        try
        {
            item.MoveTo(ActionPlanLedgerStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanLedgerStatus.Rejected, "status");
    }

    public static void ActionPlanLedgerRejectsMovesFromWithdrawn()
    {
        var item = ActionPlanLedger.NewActionPlanLedger("code-982", "title-662");
        item.Status = ActionPlanLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ActionPlanLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ActionPlanLedgerStartsInProposed();
        ActionPlanLedgerMovesToRejected();
        ActionPlanLedgerRejectsMovesFromWithdrawn();
    }
}
