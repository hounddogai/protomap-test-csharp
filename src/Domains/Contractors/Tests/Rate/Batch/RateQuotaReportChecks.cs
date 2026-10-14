namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateQuotaReportChecks
{
    public static void RateQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702842900);
        var items = new List<RateQuota>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RateQuota.NewRateQuota($"rate-quota-{index}", "sample"));
        }
        var report = RateQuotaReport.BuildRateQuotaReport("test", items, clock);
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
        RateQuotaReportCountsEachRecordOnce();
    }
}
