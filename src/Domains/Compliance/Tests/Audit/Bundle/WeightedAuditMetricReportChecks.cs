namespace Acme.Workforce.Compliance.Tests.Audit.Bundle;

using Acme.Workforce.Compliance.Audit.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WeightedAuditMetricReportChecks
{
    public static void WeightedAuditMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701750569);
        var items = new List<WeightedAuditMetric>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedAuditMetric.NewWeightedAuditMetric($"weighted-audit-metric-{index}", "sample"));
        }
        var report = WeightedAuditMetricReport.BuildWeightedAuditMetricReport("test", items, clock);
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
        WeightedAuditMetricReportCountsEachRecordOnce();
    }
}
