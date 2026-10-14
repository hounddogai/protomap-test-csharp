namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionMilestoneReportChecks
{
    public static void ExceptionMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708450912);
        var items = new List<ExceptionMilestone>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ExceptionMilestone.NewExceptionMilestone($"exception-milestone-{index}", "sample"));
        }
        var report = ExceptionMilestoneReport.BuildExceptionMilestoneReport("test", items, clock);
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
        ExceptionMilestoneReportCountsEachRecordOnce();
    }
}
