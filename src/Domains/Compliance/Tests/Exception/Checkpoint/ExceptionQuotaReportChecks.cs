namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ExceptionQuotaReportChecks
{
    public static void ExceptionQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705253867);
        var items = new List<ExceptionQuota>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ExceptionQuota.NewExceptionQuota($"exception-quota-{index}", "sample"));
        }
        var report = ExceptionQuotaReport.BuildExceptionQuotaReport("test", items, clock);
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
        ExceptionQuotaReportCountsEachRecordOnce();
    }
}
