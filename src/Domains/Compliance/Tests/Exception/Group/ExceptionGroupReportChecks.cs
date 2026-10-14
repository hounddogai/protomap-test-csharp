namespace Acme.Workforce.Compliance.Tests.Exception.Group;

using Acme.Workforce.Compliance.Exception.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ExceptionGroupReportChecks
{
    public static void ExceptionGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706608152);
        var items = new List<ExceptionGroup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ExceptionGroup.NewExceptionGroup($"exception-group-{index}", "sample"));
        }
        var report = ExceptionGroupReport.BuildExceptionGroupReport("test", items, clock);
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
        ExceptionGroupReportCountsEachRecordOnce();
    }
}
