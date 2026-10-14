namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyExceptionQuotaViewChecks
{
    public static void LegacyExceptionQuotaViewShowsTheStatusInCapitals()
    {
        var item = LegacyExceptionQuota.NewLegacyExceptionQuota("code-780", "title-762");
        var view = LegacyExceptionQuotaView.ToLegacyExceptionQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyExceptionQuotaView.ToLegacyExceptionQuotaViews(new List<LegacyExceptionQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyExceptionQuotaViewShowsTheStatusInCapitals();
    }
}
