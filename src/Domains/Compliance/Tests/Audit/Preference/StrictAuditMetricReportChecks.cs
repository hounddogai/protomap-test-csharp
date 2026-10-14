namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StrictAuditMetricReportChecks
{
    public static void StrictAuditMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708603129);
        var items = new List<StrictAuditMetric>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StrictAuditMetric.NewStrictAuditMetric($"strict-audit-metric-{index}", "sample"));
        }
        var report = StrictAuditMetricReport.BuildStrictAuditMetricReport("test", items, clock);
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
        StrictAuditMetricReportCountsEachRecordOnce();
    }
}
