namespace Acme.Workforce.Orgdesign.Tests.Headcount.Estimate;

using Acme.Workforce.Orgdesign.Headcount.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class HeadcountEstimateChecks
{
    public static void HeadcountEstimateStartsInPlanned()
    {
        var item = HeadcountEstimate.NewHeadcountEstimate("code-437", "label-491");
        Check.Equal(item.Status, HeadcountEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountEstimateMovesToInProgress()
    {
        var item = HeadcountEstimate.NewHeadcountEstimate("code-437", "label-491");
        try
        {
            item.MoveTo(HeadcountEstimateStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountEstimateStatus.InProgress, "status");
    }

    public static void HeadcountEstimateRejectsMovesFromCancelled()
    {
        var item = HeadcountEstimate.NewHeadcountEstimate("code-437", "label-491");
        item.Status = HeadcountEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(HeadcountEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        HeadcountEstimateStartsInPlanned();
        HeadcountEstimateMovesToInProgress();
        HeadcountEstimateRejectsMovesFromCancelled();
    }
}
