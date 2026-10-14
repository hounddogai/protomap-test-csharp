namespace Acme.Workforce.Payrollops.Tests.RetroPay.Window;

using Acme.Workforce.Payrollops.RetroPay.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RetroPayEstimateChecks
{
    public static void RetroPayEstimateStartsInPlanned()
    {
        var item = RetroPayEstimate.NewRetroPayEstimate("code-470", "title-326");
        Check.Equal(item.Status, RetroPayEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayEstimateMovesToInProgress()
    {
        var item = RetroPayEstimate.NewRetroPayEstimate("code-470", "title-326");
        try
        {
            item.MoveTo(RetroPayEstimateStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayEstimateStatus.InProgress, "status");
    }

    public static void RetroPayEstimateRejectsMovesFromCancelled()
    {
        var item = RetroPayEstimate.NewRetroPayEstimate("code-470", "title-326");
        item.Status = RetroPayEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RetroPayEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RetroPayEstimateStartsInPlanned();
        RetroPayEstimateMovesToInProgress();
        RetroPayEstimateRejectsMovesFromCancelled();
    }
}
