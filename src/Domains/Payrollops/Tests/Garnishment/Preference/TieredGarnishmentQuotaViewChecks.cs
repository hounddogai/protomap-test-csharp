namespace Acme.Workforce.Payrollops.Tests.Garnishment.Preference;

using Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class TieredGarnishmentQuotaViewChecks
{
    public static void TieredGarnishmentQuotaViewShowsTheStatusInCapitals()
    {
        var item = TieredGarnishmentQuota.NewTieredGarnishmentQuota("code-691", "title-669");
        var view = TieredGarnishmentQuotaView.ToTieredGarnishmentQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredGarnishmentQuotaView.ToTieredGarnishmentQuotaViews(new List<TieredGarnishmentQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredGarnishmentQuotaViewShowsTheStatusInCapitals();
    }
}
