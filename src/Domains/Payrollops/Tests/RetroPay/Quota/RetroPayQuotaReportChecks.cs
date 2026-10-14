namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayQuotaReportChecks
{
    public static void RetroPayQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709366873);
        var items = new List<RetroPayQuota>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RetroPayQuota.NewRetroPayQuota($"retro-pay-quota-{index}", "sample"));
        }
        var report = RetroPayQuotaReport.BuildRetroPayQuotaReport("test", items, clock);
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
        RetroPayQuotaReportCountsEachRecordOnce();
    }
}
