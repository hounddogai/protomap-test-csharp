namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class StrictPayBandQuotaChecks
{
    public static void StrictPayBandQuotaStartsInOpen()
    {
        var item = StrictPayBandQuota.NewStrictPayBandQuota("code-317", "label-505");
        Check.Equal(item.Status, StrictPayBandQuotaStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPayBandQuotaMovesToReopened()
    {
        var item = StrictPayBandQuota.NewStrictPayBandQuota("code-317", "label-505");
        try
        {
            item.MoveTo(StrictPayBandQuotaStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPayBandQuotaStatus.Reopened, "status");
    }

    public static void StrictPayBandQuotaRejectsMovesFromArchived()
    {
        var item = StrictPayBandQuota.NewStrictPayBandQuota("code-317", "label-505");
        item.Status = StrictPayBandQuotaStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictPayBandQuotaStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictPayBandQuotaStartsInOpen();
        StrictPayBandQuotaMovesToReopened();
        StrictPayBandQuotaRejectsMovesFromArchived();
    }
}
