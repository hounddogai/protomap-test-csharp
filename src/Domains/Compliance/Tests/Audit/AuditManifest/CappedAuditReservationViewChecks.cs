namespace Acme.Workforce.Compliance.Tests.Audit.AuditManifest;

using Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>Checks of audit manifest code, run by the project's runner.</summary>
public static class CappedAuditReservationViewChecks
{
    public static void CappedAuditReservationViewShowsTheStatusInCapitals()
    {
        var item = CappedAuditReservation.NewCappedAuditReservation("code-212", "name-839");
        var view = CappedAuditReservationView.ToCappedAuditReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedAuditReservationView.ToCappedAuditReservationViews(new List<CappedAuditReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedAuditReservationViewShowsTheStatusInCapitals();
    }
}
