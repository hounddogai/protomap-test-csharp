namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayThresholdChecks
{
    public static void RetroPayThresholdStartsInPlanned()
    {
        var item = RetroPayThreshold.NewRetroPayThreshold("code-950", "label-496");
        Check.Equal(item.Status, RetroPayThresholdStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayThresholdMovesToInProgress()
    {
        var item = RetroPayThreshold.NewRetroPayThreshold("code-950", "label-496");
        try
        {
            item.MoveTo(RetroPayThresholdStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayThresholdStatus.InProgress, "status");
    }

    public static void RetroPayThresholdRejectsMovesFromCancelled()
    {
        var item = RetroPayThreshold.NewRetroPayThreshold("code-950", "label-496");
        item.Status = RetroPayThresholdStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RetroPayThresholdStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RetroPayThresholdStartsInPlanned();
        RetroPayThresholdMovesToInProgress();
        RetroPayThresholdRejectsMovesFromCancelled();
    }
}
