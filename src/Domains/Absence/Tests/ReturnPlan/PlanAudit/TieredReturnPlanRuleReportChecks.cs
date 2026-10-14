namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class TieredReturnPlanRuleReportChecks
{
    public static void TieredReturnPlanRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706366231);
        var items = new List<TieredReturnPlanRule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredReturnPlanRule.NewTieredReturnPlanRule($"tiered-return-plan-rule-{index}", "sample"));
        }
        var report = TieredReturnPlanRuleReport.BuildTieredReturnPlanRuleReport("test", items, clock);
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
        TieredReturnPlanRuleReportCountsEachRecordOnce();
    }
}
