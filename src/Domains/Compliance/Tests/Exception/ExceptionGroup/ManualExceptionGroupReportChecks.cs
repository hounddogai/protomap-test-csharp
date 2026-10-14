namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionGroup;

using Acme.Workforce.Compliance.Exception.ExceptionGroup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception group code, run by the project's runner.</summary>
public static class ManualExceptionGroupReportChecks
{
    public static void ManualExceptionGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708952460);
        var items = new List<ManualExceptionGroup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ManualExceptionGroup.NewManualExceptionGroup($"manual-exception-group-{index}", "sample"));
        }
        var report = ManualExceptionGroupReport.BuildManualExceptionGroupReport("test", items, clock);
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
        ManualExceptionGroupReportCountsEachRecordOnce();
    }
}
