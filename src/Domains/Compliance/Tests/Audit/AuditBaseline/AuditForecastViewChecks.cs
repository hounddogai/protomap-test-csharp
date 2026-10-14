namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class AuditForecastViewChecks
{
    public static void AuditForecastViewShowsTheStatusInCapitals()
    {
        var item = AuditForecast.NewAuditForecast("code-114", "name-134");
        var view = AuditForecastView.ToAuditForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditForecastView.ToAuditForecastViews(new List<AuditForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditForecastViewShowsTheStatusInCapitals();
    }
}
