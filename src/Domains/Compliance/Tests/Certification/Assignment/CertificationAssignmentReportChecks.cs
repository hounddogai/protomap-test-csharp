namespace Acme.Workforce.Compliance.Tests.Certification.Assignment;

using Acme.Workforce.Compliance.Certification.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CertificationAssignmentReportChecks
{
    public static void CertificationAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706388157);
        var items = new List<CertificationAssignment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CertificationAssignment.NewCertificationAssignment($"certification-assignment-{index}", "sample"));
        }
        var report = CertificationAssignmentReport.BuildCertificationAssignmentReport("test", items, clock);
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
        CertificationAssignmentReportCountsEachRecordOnce();
    }
}
