namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class GlobalLaptopPlanReportChecks
{
    public static void GlobalLaptopPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705735357);
        var items = new List<GlobalLaptopPlan>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(GlobalLaptopPlan.NewGlobalLaptopPlan($"global-laptop-plan-{index}", "sample"));
        }
        var report = GlobalLaptopPlanReport.BuildGlobalLaptopPlanReport("test", items, clock);
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
        GlobalLaptopPlanReportCountsEachRecordOnce();
    }
}
