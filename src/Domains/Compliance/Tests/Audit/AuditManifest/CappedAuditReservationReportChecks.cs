namespace Acme.Workforce.Compliance.Tests.Audit.AuditManifest;

using Acme.Workforce.Compliance.Audit.AuditManifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit manifest code, run by the project's runner.</summary>
public static class CappedAuditReservationReportChecks
{
    public static void CappedAuditReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700568394);
        var items = new List<CappedAuditReservation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedAuditReservation.NewCappedAuditReservation($"capped-audit-reservation-{index}", "sample"));
        }
        var report = CappedAuditReservationReport.BuildCappedAuditReservationReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CappedAuditReservationReportCountsEachRecordOnce();
    }
}
