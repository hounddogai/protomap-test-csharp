namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GlobalMobilityRuleReportChecks
{
    public static void GlobalMobilityRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702767844);
        var items = new List<GlobalMobilityRule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(GlobalMobilityRule.NewGlobalMobilityRule($"global-mobility-rule-{index}", "sample"));
        }
        var report = GlobalMobilityRuleReport.BuildGlobalMobilityRuleReport("test", items, clock);
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
        GlobalMobilityRuleReportCountsEachRecordOnce();
    }
}
