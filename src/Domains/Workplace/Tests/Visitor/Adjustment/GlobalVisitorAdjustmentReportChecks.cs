namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalVisitorAdjustmentReportChecks
{
    public static void GlobalVisitorAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706745541);
        var items = new List<GlobalVisitorAdjustment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(GlobalVisitorAdjustment.NewGlobalVisitorAdjustment($"global-visitor-adjustment-{index}", "sample"));
        }
        var report = GlobalVisitorAdjustmentReport.BuildGlobalVisitorAdjustmentReport("test", items, clock);
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
        GlobalVisitorAdjustmentReportCountsEachRecordOnce();
    }
}
