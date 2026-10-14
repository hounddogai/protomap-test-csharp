namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledReorgExceptionReportChecks
{
    public static void ScheduledReorgExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705267697);
        var items = new List<ScheduledReorgException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ScheduledReorgException.NewScheduledReorgException($"scheduled-reorg-exception-{index}", "sample"));
        }
        var report = ScheduledReorgExceptionReport.BuildScheduledReorgExceptionReport("test", items, clock);
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
        ScheduledReorgExceptionReportCountsEachRecordOnce();
    }
}
