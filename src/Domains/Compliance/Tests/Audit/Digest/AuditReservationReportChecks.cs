namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditReservationReportChecks
{
    public static void AuditReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707979442);
        var items = new List<AuditReservation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AuditReservation.NewAuditReservation($"audit-reservation-{index}", "sample"));
        }
        var report = AuditReservationReport.BuildAuditReservationReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AuditReservationReportCountsEachRecordOnce();
    }
}
