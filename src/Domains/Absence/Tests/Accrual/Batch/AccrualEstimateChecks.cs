namespace Acme.Workforce.Absence.Tests.Accrual.Batch;

using Acme.Workforce.Absence.Accrual.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccrualEstimateChecks
{
    public static void AccrualEstimateStartsInDraft()
    {
        var item = AccrualEstimate.NewAccrualEstimate("code-344", "name-374");
        Check.Equal(item.Status, AccrualEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualEstimateMovesToPending()
    {
        var item = AccrualEstimate.NewAccrualEstimate("code-344", "name-374");
        try
        {
            item.MoveTo(AccrualEstimateStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualEstimateStatus.Pending, "status");
    }

    public static void AccrualEstimateRejectsMovesFromClosed()
    {
        var item = AccrualEstimate.NewAccrualEstimate("code-344", "name-374");
        item.Status = AccrualEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(AccrualEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccrualEstimateStartsInDraft();
        AccrualEstimateMovesToPending();
        AccrualEstimateRejectsMovesFromClosed();
    }
}
