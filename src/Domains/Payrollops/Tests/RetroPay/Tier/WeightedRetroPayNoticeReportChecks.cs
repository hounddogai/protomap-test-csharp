namespace Acme.Workforce.Payrollops.Tests.RetroPay.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class WeightedRetroPayNoticeReportChecks
{
    public static void WeightedRetroPayNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701623882);
        var items = new List<WeightedRetroPayNotice>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedRetroPayNotice.NewWeightedRetroPayNotice($"weighted-retro-pay-notice-{index}", "sample"));
        }
        var report = WeightedRetroPayNoticeReport.BuildWeightedRetroPayNoticeReport("test", items, clock);
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
        WeightedRetroPayNoticeReportCountsEachRecordOnce();
    }
}
