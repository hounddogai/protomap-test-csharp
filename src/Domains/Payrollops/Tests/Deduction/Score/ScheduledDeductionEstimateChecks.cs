namespace Acme.Workforce.Payrollops.Tests.Deduction.Score;

using Acme.Workforce.Payrollops.Deduction.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledDeductionEstimateChecks
{
    public static void ScheduledDeductionEstimateStartsInIdle()
    {
        var item = ScheduledDeductionEstimate.NewScheduledDeductionEstimate("code-558", "name-565");
        Check.Equal(item.Status, ScheduledDeductionEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledDeductionEstimateMovesToReady()
    {
        var item = ScheduledDeductionEstimate.NewScheduledDeductionEstimate("code-558", "name-565");
        try
        {
            item.MoveTo(ScheduledDeductionEstimateStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledDeductionEstimateStatus.Ready, "status");
    }

    public static void ScheduledDeductionEstimateRejectsMovesFromStopped()
    {
        var item = ScheduledDeductionEstimate.NewScheduledDeductionEstimate("code-558", "name-565");
        item.Status = ScheduledDeductionEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledDeductionEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledDeductionEstimateStartsInIdle();
        ScheduledDeductionEstimateMovesToReady();
        ScheduledDeductionEstimateRejectsMovesFromStopped();
    }
}
