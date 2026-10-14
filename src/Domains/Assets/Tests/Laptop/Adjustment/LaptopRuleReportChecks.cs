namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LaptopRuleReportChecks
{
    public static void LaptopRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703256674);
        var items = new List<LaptopRule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LaptopRule.NewLaptopRule($"laptop-rule-{index}", "sample"));
        }
        var report = LaptopRuleReport.BuildLaptopRuleReport("test", items, clock);
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
        LaptopRuleReportCountsEachRecordOnce();
    }
}
