namespace Acme.Workforce.Compliance.Tests.Audit.Manifest;

using Acme.Workforce.Compliance.Audit.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AuditEstimateViewChecks
{
    public static void AuditEstimateViewShowsTheStatusInCapitals()
    {
        var item = AuditEstimate.NewAuditEstimate("code-527", "name-10");
        var view = AuditEstimateView.ToAuditEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditEstimateView.ToAuditEstimateViews(new List<AuditEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditEstimateViewShowsTheStatusInCapitals();
    }
}
