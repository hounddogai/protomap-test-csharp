namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingExceptionQuotaReportChecks
{
    public static void PendingExceptionQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704109051);
        var items = new List<PendingExceptionQuota>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingExceptionQuota.NewPendingExceptionQuota($"pending-exception-quota-{index}", "sample"));
        }
        var report = PendingExceptionQuotaReport.BuildPendingExceptionQuotaReport("test", items, clock);
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
        PendingExceptionQuotaReportCountsEachRecordOnce();
    }
}
