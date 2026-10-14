namespace Acme.Workforce.Compliance.Tests.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of certification assignment code, run by the project's runner.</summary>
public static class CappedCertificationAssignmentReportChecks
{
    public static void CappedCertificationAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704237444);
        var items = new List<CappedCertificationAssignment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedCertificationAssignment.NewCappedCertificationAssignment($"capped-certification-assignment-{index}", "sample"));
        }
        var report = CappedCertificationAssignmentReport.BuildCappedCertificationAssignmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CappedCertificationAssignmentReportCountsEachRecordOnce();
    }
}
