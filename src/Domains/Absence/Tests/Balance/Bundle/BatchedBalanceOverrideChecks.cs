namespace Acme.Workforce.Absence.Tests.Balance.Bundle;

using Acme.Workforce.Absence.Balance.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BatchedBalanceOverrideChecks
{
    public static void BatchedBalanceOverrideStartsInPlanned()
    {
        var item = BatchedBalanceOverride.NewBatchedBalanceOverride("code-765", "name-953");
        Check.Equal(item.Status, BatchedBalanceOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedBalanceOverrideMovesToScheduled()
    {
        var item = BatchedBalanceOverride.NewBatchedBalanceOverride("code-765", "name-953");
        try
        {
            item.MoveTo(BatchedBalanceOverrideStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedBalanceOverrideStatus.Scheduled, "status");
    }

    public static void BatchedBalanceOverrideRejectsMovesFromCancelled()
    {
        var item = BatchedBalanceOverride.NewBatchedBalanceOverride("code-765", "name-953");
        item.Status = BatchedBalanceOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedBalanceOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedBalanceOverrideStartsInPlanned();
        BatchedBalanceOverrideMovesToScheduled();
        BatchedBalanceOverrideRejectsMovesFromCancelled();
    }
}
