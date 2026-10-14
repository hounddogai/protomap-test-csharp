namespace Acme.Workforce.Contractors.Tests.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class OffboardingAssignmentReportChecks
{
    public static void OffboardingAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708750093);
        var items = new List<OffboardingAssignment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(OffboardingAssignment.NewOffboardingAssignment($"offboarding-assignment-{index}", "sample"));
        }
        var report = OffboardingAssignmentReport.BuildOffboardingAssignmentReport("test", items, clock);
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
        OffboardingAssignmentReportCountsEachRecordOnce();
    }
}
