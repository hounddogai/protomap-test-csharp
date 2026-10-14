namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayQuotaViewChecks
{
    public static void RetroPayQuotaViewShowsTheStatusInCapitals()
    {
        var item = RetroPayQuota.NewRetroPayQuota("code-261", "name-747");
        var view = RetroPayQuotaView.ToRetroPayQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayQuotaView.ToRetroPayQuotaViews(new List<RetroPayQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayQuotaViewShowsTheStatusInCapitals();
    }
}
