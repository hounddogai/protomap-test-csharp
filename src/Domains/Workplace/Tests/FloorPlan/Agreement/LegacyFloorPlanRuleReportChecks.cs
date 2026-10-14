namespace Acme.Workforce.Workplace.Tests.FloorPlan.Agreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LegacyFloorPlanRuleReportChecks
{
    public static void LegacyFloorPlanRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706929604);
        var items = new List<LegacyFloorPlanRule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LegacyFloorPlanRule.NewLegacyFloorPlanRule($"legacy-floor-plan-rule-{index}", "sample"));
        }
        var report = LegacyFloorPlanRuleReport.BuildLegacyFloorPlanRuleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LegacyFloorPlanRuleReportCountsEachRecordOnce();
    }
}
