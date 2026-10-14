namespace Acme.Workforce.Payrollops.Tests.Garnishment.RecordItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GarnishmentThresholdReportChecks
{
    public static void GarnishmentThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708875376);
        var items = new List<GarnishmentThreshold>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(GarnishmentThreshold.NewGarnishmentThreshold($"garnishment-threshold-{index}", "sample"));
        }
        var report = GarnishmentThresholdReport.BuildGarnishmentThresholdReport("test", items, clock);
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
        GarnishmentThresholdReportCountsEachRecordOnce();
    }
}
