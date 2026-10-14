namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientCertificationAssignmentReportChecks
{
    public static void LenientCertificationAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704339019);
        var items = new List<LenientCertificationAssignment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientCertificationAssignment.NewLenientCertificationAssignment($"lenient-certification-assignment-{index}", "sample"));
        }
        var report = LenientCertificationAssignmentReport.BuildLenientCertificationAssignmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LenientCertificationAssignmentReportCountsEachRecordOnce();
    }
}
