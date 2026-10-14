namespace Acme.Workforce.Payrollops.Tests.Garnishment.Preference;

using Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class TieredGarnishmentQuotaChecks
{
    public static void TieredGarnishmentQuotaStartsInOpen()
    {
        var item = TieredGarnishmentQuota.NewTieredGarnishmentQuota("code-691", "title-669");
        Check.Equal(item.Status, TieredGarnishmentQuotaStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredGarnishmentQuotaMovesToReopened()
    {
        var item = TieredGarnishmentQuota.NewTieredGarnishmentQuota("code-691", "title-669");
        try
        {
            item.MoveTo(TieredGarnishmentQuotaStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredGarnishmentQuotaStatus.Reopened, "status");
    }

    public static void TieredGarnishmentQuotaRejectsMovesFromArchived()
    {
        var item = TieredGarnishmentQuota.NewTieredGarnishmentQuota("code-691", "title-669");
        item.Status = TieredGarnishmentQuotaStatus.Archived;
        Check.Fails(() => item.MoveTo(TieredGarnishmentQuotaStatus.Open), "move");
    }

    public static void RunAll()
    {
        TieredGarnishmentQuotaStartsInOpen();
        TieredGarnishmentQuotaMovesToReopened();
        TieredGarnishmentQuotaRejectsMovesFromArchived();
    }
}
