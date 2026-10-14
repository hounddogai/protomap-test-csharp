namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class TieredHeadcountEstimateChecks
{
    public static void TieredHeadcountEstimateStartsInPlanned()
    {
        var item = TieredHeadcountEstimate.NewTieredHeadcountEstimate("code-601", "name-701");
        Check.Equal(item.Status, TieredHeadcountEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredHeadcountEstimateMovesToCompleted()
    {
        var item = TieredHeadcountEstimate.NewTieredHeadcountEstimate("code-601", "name-701");
        try
        {
            item.MoveTo(TieredHeadcountEstimateStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredHeadcountEstimateStatus.Completed, "status");
    }

    public static void TieredHeadcountEstimateRejectsMovesFromCancelled()
    {
        var item = TieredHeadcountEstimate.NewTieredHeadcountEstimate("code-601", "name-701");
        item.Status = TieredHeadcountEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredHeadcountEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredHeadcountEstimateStartsInPlanned();
        TieredHeadcountEstimateMovesToCompleted();
        TieredHeadcountEstimateRejectsMovesFromCancelled();
    }
}
