namespace Acme.Workforce.Compensation.Tests.EquityGrant.Quota;

using Acme.Workforce.Compensation.EquityGrant.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScheduledEquityGrantQuotaReportChecks
{
    public static void ScheduledEquityGrantQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703537197);
        var items = new List<ScheduledEquityGrantQuota>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota($"scheduled-equity-grant-quota-{index}", "sample"));
        }
        var report = ScheduledEquityGrantQuotaReport.BuildScheduledEquityGrantQuotaReport("test", items, clock);
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
        ScheduledEquityGrantQuotaReportCountsEachRecordOnce();
    }
}
