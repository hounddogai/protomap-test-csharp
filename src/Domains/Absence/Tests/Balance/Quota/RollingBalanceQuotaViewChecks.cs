namespace Acme.Workforce.Absence.Tests.Balance.Quota;

using Acme.Workforce.Absence.Balance.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RollingBalanceQuotaViewChecks
{
    public static void RollingBalanceQuotaViewShowsTheStatusInCapitals()
    {
        var item = RollingBalanceQuota.NewRollingBalanceQuota("code-773", "title-724");
        var view = RollingBalanceQuotaView.ToRollingBalanceQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingBalanceQuotaView.ToRollingBalanceQuotaViews(new List<RollingBalanceQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingBalanceQuotaViewShowsTheStatusInCapitals();
    }
}
