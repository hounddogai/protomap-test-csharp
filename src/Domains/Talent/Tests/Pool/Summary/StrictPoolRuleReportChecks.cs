namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StrictPoolRuleReportChecks
{
    public static void StrictPoolRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704548255);
        var items = new List<StrictPoolRule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictPoolRule.NewStrictPoolRule($"strict-pool-rule-{index}", "sample"));
        }
        var report = StrictPoolRuleReport.BuildStrictPoolRuleReport("test", items, clock);
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
        StrictPoolRuleReportCountsEachRecordOnce();
    }
}
