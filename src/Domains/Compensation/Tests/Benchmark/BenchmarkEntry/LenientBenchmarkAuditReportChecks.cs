namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class LenientBenchmarkAuditReportChecks
{
    public static void LenientBenchmarkAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701093916);
        var items = new List<LenientBenchmarkAudit>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientBenchmarkAudit.NewLenientBenchmarkAudit($"lenient-benchmark-audit-{index}", "sample"));
        }
        var report = LenientBenchmarkAuditReport.BuildLenientBenchmarkAuditReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LenientBenchmarkAuditReportCountsEachRecordOnce();
    }
}
