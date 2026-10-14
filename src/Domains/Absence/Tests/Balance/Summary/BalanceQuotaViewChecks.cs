namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BalanceQuotaViewChecks
{
    public static void BalanceQuotaViewShowsTheStatusInCapitals()
    {
        var item = BalanceQuota.NewBalanceQuota("code-585", "label-558");
        var view = BalanceQuotaView.ToBalanceQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceQuotaView.ToBalanceQuotaViews(new List<BalanceQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceQuotaViewShowsTheStatusInCapitals();
    }
}
