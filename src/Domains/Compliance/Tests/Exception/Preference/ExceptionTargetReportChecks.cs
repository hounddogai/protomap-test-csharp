namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ExceptionTargetReportChecks
{
    public static void ExceptionTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707181908);
        var items = new List<ExceptionTarget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ExceptionTarget.NewExceptionTarget($"exception-target-{index}", "sample"));
        }
        var report = ExceptionTargetReport.BuildExceptionTargetReport("test", items, clock);
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
        ExceptionTargetReportCountsEachRecordOnce();
    }
}
