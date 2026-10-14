namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DefaultVisitorRuleReportChecks
{
    public static void DefaultVisitorRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706338930);
        var items = new List<DefaultVisitorRule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DefaultVisitorRule.NewDefaultVisitorRule($"default-visitor-rule-{index}", "sample"));
        }
        var report = DefaultVisitorRuleReport.BuildDefaultVisitorRuleReport("test", items, clock);
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
        DefaultVisitorRuleReportCountsEachRecordOnce();
    }
}
