namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class CappedPayRunTargetReportChecks
{
    public static void CappedPayRunTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706001292);
        var items = new List<CappedPayRunTarget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedPayRunTarget.NewCappedPayRunTarget($"capped-pay-run-target-{index}", "sample"));
        }
        var report = CappedPayRunTargetReport.BuildCappedPayRunTargetReport("test", items, clock);
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
        CappedPayRunTargetReportCountsEachRecordOnce();
    }
}
