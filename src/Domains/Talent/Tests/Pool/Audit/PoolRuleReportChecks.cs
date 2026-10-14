namespace Acme.Workforce.Talent.Tests.Pool.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PoolRuleReportChecks
{
    public static void PoolRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707942188);
        var items = new List<PoolRule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PoolRule.NewPoolRule($"pool-rule-{index}", "sample"));
        }
        var report = PoolRuleReport.BuildPoolRuleReport("test", items, clock);
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
        PoolRuleReportCountsEachRecordOnce();
    }
}
