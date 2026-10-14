namespace Acme.Workforce.Payrollops.Tests.Garnishment.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackGarnishmentRecordReportChecks
{
    public static void FallbackGarnishmentRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701737063);
        var items = new List<FallbackGarnishmentRecord>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackGarnishmentRecord.NewFallbackGarnishmentRecord($"fallback-garnishment-record-{index}", "sample"));
        }
        var report = FallbackGarnishmentRecordReport.BuildFallbackGarnishmentRecordReport("test", items, clock);
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
        FallbackGarnishmentRecordReportCountsEachRecordOnce();
    }
}
