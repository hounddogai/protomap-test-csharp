namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateQuotaChecks
{
    public static void RateQuotaStartsInPlanned()
    {
        var item = RateQuota.NewRateQuota("code-349", "name-915");
        Check.Equal(item.Status, RateQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateQuotaMovesToInProgress()
    {
        var item = RateQuota.NewRateQuota("code-349", "name-915");
        try
        {
            item.MoveTo(RateQuotaStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateQuotaStatus.InProgress, "status");
    }

    public static void RateQuotaRejectsMovesFromCancelled()
    {
        var item = RateQuota.NewRateQuota("code-349", "name-915");
        item.Status = RateQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RateQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RateQuotaStartsInPlanned();
        RateQuotaMovesToInProgress();
        RateQuotaRejectsMovesFromCancelled();
    }
}
