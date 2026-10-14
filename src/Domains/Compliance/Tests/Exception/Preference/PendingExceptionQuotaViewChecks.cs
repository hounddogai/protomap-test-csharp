namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingExceptionQuotaViewChecks
{
    public static void PendingExceptionQuotaViewShowsTheStatusInCapitals()
    {
        var item = PendingExceptionQuota.NewPendingExceptionQuota("code-599", "name-932");
        var view = PendingExceptionQuotaView.ToPendingExceptionQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingExceptionQuotaView.ToPendingExceptionQuotaViews(new List<PendingExceptionQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingExceptionQuotaViewShowsTheStatusInCapitals();
    }
}
