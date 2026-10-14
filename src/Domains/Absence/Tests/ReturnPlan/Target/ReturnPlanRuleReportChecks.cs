namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanRuleReportChecks
{
    public static void ReturnPlanRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701436665);
        var items = new List<ReturnPlanRule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ReturnPlanRule.NewReturnPlanRule($"return-plan-rule-{index}", "sample"));
        }
        var report = ReturnPlanRuleReport.BuildReturnPlanRuleReport("test", items, clock);
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
        ReturnPlanRuleReportCountsEachRecordOnce();
    }
}
