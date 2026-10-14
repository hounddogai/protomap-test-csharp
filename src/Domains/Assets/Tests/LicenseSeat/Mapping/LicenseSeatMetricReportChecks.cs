namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatMetricReportChecks
{
    public static void LicenseSeatMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706038238);
        var items = new List<LicenseSeatMetric>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LicenseSeatMetric.NewLicenseSeatMetric($"license-seat-metric-{index}", "sample"));
        }
        var report = LicenseSeatMetricReport.BuildLicenseSeatMetricReport("test", items, clock);
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
        LicenseSeatMetricReportCountsEachRecordOnce();
    }
}
