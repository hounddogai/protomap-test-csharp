namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DefaultExceptionQuotaReportChecks
{
    public static void DefaultExceptionQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704121338);
        var items = new List<DefaultExceptionQuota>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultExceptionQuota.NewDefaultExceptionQuota($"default-exception-quota-{index}", "sample"));
        }
        var report = DefaultExceptionQuotaReport.BuildDefaultExceptionQuotaReport("test", items, clock);
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
        DefaultExceptionQuotaReportCountsEachRecordOnce();
    }
}
