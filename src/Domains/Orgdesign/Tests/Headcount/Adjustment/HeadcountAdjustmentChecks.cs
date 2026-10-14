namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HeadcountAdjustmentChecks
{
    public static void HeadcountAdjustmentStartsInPlanned()
    {
        var item = HeadcountAdjustment.NewHeadcountAdjustment("code-492", "title-468");
        Check.Equal(item.Status, HeadcountAdjustmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountAdjustmentMovesToInProgress()
    {
        var item = HeadcountAdjustment.NewHeadcountAdjustment("code-492", "title-468");
        try
        {
            item.MoveTo(HeadcountAdjustmentStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountAdjustmentStatus.InProgress, "status");
    }

    public static void HeadcountAdjustmentRejectsMovesFromCancelled()
    {
        var item = HeadcountAdjustment.NewHeadcountAdjustment("code-492", "title-468");
        item.Status = HeadcountAdjustmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(HeadcountAdjustmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        HeadcountAdjustmentStartsInPlanned();
        HeadcountAdjustmentMovesToInProgress();
        HeadcountAdjustmentRejectsMovesFromCancelled();
    }
}
