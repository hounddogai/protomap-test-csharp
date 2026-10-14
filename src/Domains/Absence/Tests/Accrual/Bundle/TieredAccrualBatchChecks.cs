namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredAccrualBatchChecks
{
    public static void TieredAccrualBatchStartsInCreated()
    {
        var item = TieredAccrualBatch.NewTieredAccrualBatch("code-316", "label-794");
        Check.Equal(item.Status, TieredAccrualBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredAccrualBatchMovesToPublished()
    {
        var item = TieredAccrualBatch.NewTieredAccrualBatch("code-316", "label-794");
        try
        {
            item.MoveTo(TieredAccrualBatchStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredAccrualBatchStatus.Published, "status");
    }

    public static void TieredAccrualBatchRejectsMovesFromRetired()
    {
        var item = TieredAccrualBatch.NewTieredAccrualBatch("code-316", "label-794");
        item.Status = TieredAccrualBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredAccrualBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredAccrualBatchStartsInCreated();
        TieredAccrualBatchMovesToPublished();
        TieredAccrualBatchRejectsMovesFromRetired();
    }
}
