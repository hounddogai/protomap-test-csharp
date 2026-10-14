namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalPayBandQuotaChecks
{
    public static void SeasonalPayBandQuotaStartsInIdle()
    {
        var item = SeasonalPayBandQuota.NewSeasonalPayBandQuota("code-589", "label-24");
        Check.Equal(item.Status, SeasonalPayBandQuotaStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPayBandQuotaMovesToWarming()
    {
        var item = SeasonalPayBandQuota.NewSeasonalPayBandQuota("code-589", "label-24");
        try
        {
            item.MoveTo(SeasonalPayBandQuotaStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPayBandQuotaStatus.Warming, "status");
    }

    public static void SeasonalPayBandQuotaRejectsMovesFromStopped()
    {
        var item = SeasonalPayBandQuota.NewSeasonalPayBandQuota("code-589", "label-24");
        item.Status = SeasonalPayBandQuotaStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalPayBandQuotaStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalPayBandQuotaStartsInIdle();
        SeasonalPayBandQuotaMovesToWarming();
        SeasonalPayBandQuotaRejectsMovesFromStopped();
    }
}
