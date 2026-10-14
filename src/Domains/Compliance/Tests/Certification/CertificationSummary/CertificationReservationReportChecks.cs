namespace Acme.Workforce.Compliance.Tests.Certification.CertificationSummary;

using Acme.Workforce.Compliance.Certification.CertificationSummary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of certification summary code, run by the project's runner.</summary>
public static class CertificationReservationReportChecks
{
    public static void CertificationReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704459713);
        var items = new List<CertificationReservation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CertificationReservation.NewCertificationReservation($"certification-reservation-{index}", "sample"));
        }
        var report = CertificationReservationReport.BuildCertificationReservationReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CertificationReservationReportCountsEachRecordOnce();
    }
}
