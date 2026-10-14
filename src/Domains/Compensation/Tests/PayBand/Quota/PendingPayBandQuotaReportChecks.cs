namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PendingPayBandQuotaReportChecks
{
    public static void PendingPayBandQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705101624);
        var items = new List<PendingPayBandQuota>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingPayBandQuota.NewPendingPayBandQuota($"pending-pay-band-quota-{index}", "sample"));
        }
        var report = PendingPayBandQuotaReport.BuildPendingPayBandQuotaReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PendingPayBandQuotaReportCountsEachRecordOnce();
    }
}
