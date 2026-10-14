namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBudget;

using Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Checks of deduction budget code, run by the project's runner.</summary>
public static class BatchedDeductionBudgetChecks
{
    public static void BatchedDeductionBudgetStartsInRequested()
    {
        var item = BatchedDeductionBudget.NewBatchedDeductionBudget("code-393", "label-606");
        Check.Equal(item.Status, BatchedDeductionBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedDeductionBudgetMovesToGranted()
    {
        var item = BatchedDeductionBudget.NewBatchedDeductionBudget("code-393", "label-606");
        try
        {
            item.MoveTo(BatchedDeductionBudgetStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedDeductionBudgetStatus.Granted, "status");
    }

    public static void BatchedDeductionBudgetRejectsMovesFromExpired()
    {
        var item = BatchedDeductionBudget.NewBatchedDeductionBudget("code-393", "label-606");
        item.Status = BatchedDeductionBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedDeductionBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedDeductionBudgetStartsInRequested();
        BatchedDeductionBudgetMovesToGranted();
        BatchedDeductionBudgetRejectsMovesFromExpired();
    }
}
