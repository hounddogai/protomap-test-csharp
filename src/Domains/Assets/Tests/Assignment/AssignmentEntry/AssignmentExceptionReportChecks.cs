namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentEntry;

using Acme.Workforce.Assets.Assignment.AssignmentEntry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment entry code, run by the project's runner.</summary>
public static class AssignmentExceptionReportChecks
{
    public static void AssignmentExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703865894);
        var items = new List<AssignmentException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssignmentException.NewAssignmentException($"assignment-exception-{index}", "sample"));
        }
        var report = AssignmentExceptionReport.BuildAssignmentExceptionReport("test", items, clock);
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
        AssignmentExceptionReportCountsEachRecordOnce();
    }
}
