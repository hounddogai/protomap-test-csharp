namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DefaultExceptionQuotaViewChecks
{
    public static void DefaultExceptionQuotaViewShowsTheStatusInCapitals()
    {
        var item = DefaultExceptionQuota.NewDefaultExceptionQuota("code-293", "title-62");
        var view = DefaultExceptionQuotaView.ToDefaultExceptionQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultExceptionQuotaView.ToDefaultExceptionQuotaViews(new List<DefaultExceptionQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultExceptionQuotaViewShowsTheStatusInCapitals();
    }
}
