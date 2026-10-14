namespace Acme.Workforce.Compliance.Tests.Exception.Assignment;

using Acme.Workforce.Compliance.Exception.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ExceptionAssignmentReportChecks
{
    public static void ExceptionAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703236912);
        var items = new List<ExceptionAssignment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ExceptionAssignment.NewExceptionAssignment($"exception-assignment-{index}", "sample"));
        }
        var report = ExceptionAssignmentReport.BuildExceptionAssignmentReport("test", items, clock);
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
        ExceptionAssignmentReportCountsEachRecordOnce();
    }
}
