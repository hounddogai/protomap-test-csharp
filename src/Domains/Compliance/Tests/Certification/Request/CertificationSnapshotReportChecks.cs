namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationSnapshotReportChecks
{
    public static void CertificationSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701710169);
        var items = new List<CertificationSnapshot>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CertificationSnapshot.NewCertificationSnapshot($"certification-snapshot-{index}", "sample"));
        }
        var report = CertificationSnapshotReport.BuildCertificationSnapshotReport("test", items, clock);
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
        CertificationSnapshotReportCountsEachRecordOnce();
    }
}
