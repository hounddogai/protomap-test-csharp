namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedBalanceRuleChecks
{
    public static void BatchedBalanceRuleStartsInProposed()
    {
        var item = BatchedBalanceRule.NewBatchedBalanceRule("code-591", "name-72");
        Check.Equal(item.Status, BatchedBalanceRuleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedBalanceRuleMovesToRejected()
    {
        var item = BatchedBalanceRule.NewBatchedBalanceRule("code-591", "name-72");
        try
        {
            item.MoveTo(BatchedBalanceRuleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedBalanceRuleStatus.Rejected, "status");
    }

    public static void BatchedBalanceRuleRejectsMovesFromWithdrawn()
    {
        var item = BatchedBalanceRule.NewBatchedBalanceRule("code-591", "name-72");
        item.Status = BatchedBalanceRuleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BatchedBalanceRuleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BatchedBalanceRuleStartsInProposed();
        BatchedBalanceRuleMovesToRejected();
        BatchedBalanceRuleRejectsMovesFromWithdrawn();
    }
}
