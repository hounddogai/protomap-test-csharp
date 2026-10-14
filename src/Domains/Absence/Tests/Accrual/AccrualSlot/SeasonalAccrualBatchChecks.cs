namespace Acme.Workforce.Absence.Tests.Accrual.AccrualSlot;

using Acme.Workforce.Absence.Accrual.AccrualSlot;

/// <summary>Checks of accrual slot code, run by the project's runner.</summary>
public static class SeasonalAccrualBatchChecks
{
    public static void SeasonalAccrualBatchStartsInPlanned()
    {
        var item = SeasonalAccrualBatch.NewSeasonalAccrualBatch("code-663", "name-791");
        Check.Equal(item.Status, SeasonalAccrualBatchStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAccrualBatchMovesToCancelled()
    {
        var item = SeasonalAccrualBatch.NewSeasonalAccrualBatch("code-663", "name-791");
        try
        {
            item.MoveTo(SeasonalAccrualBatchStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAccrualBatchStatus.Cancelled, "status");
    }

    public static void SeasonalAccrualBatchRejectsMovesFromCancelled()
    {
        var item = SeasonalAccrualBatch.NewSeasonalAccrualBatch("code-663", "name-791");
        item.Status = SeasonalAccrualBatchStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalAccrualBatchStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalAccrualBatchStartsInPlanned();
        SeasonalAccrualBatchMovesToCancelled();
        SeasonalAccrualBatchRejectsMovesFromCancelled();
    }
}
