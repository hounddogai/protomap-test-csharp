namespace Acme.Workforce.Payrollops.Tests.RetroPay.Claim;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AdaptiveRetroPayMetricReportChecks
{
    public static void AdaptiveRetroPayMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703664799);
        var items = new List<AdaptiveRetroPayMetric>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric($"adaptive-retro-pay-metric-{index}", "sample"));
        }
        var report = AdaptiveRetroPayMetricReport.BuildAdaptiveRetroPayMetricReport("test", items, clock);
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
        AdaptiveRetroPayMetricReportCountsEachRecordOnce();
    }
}
