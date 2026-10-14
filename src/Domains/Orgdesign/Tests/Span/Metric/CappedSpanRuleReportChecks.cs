namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedSpanRuleReportChecks
{
    public static void CappedSpanRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702325293);
        var items = new List<CappedSpanRule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CappedSpanRule.NewCappedSpanRule($"capped-span-rule-{index}", "sample"));
        }
        var report = CappedSpanRuleReport.BuildCappedSpanRuleReport("test", items, clock);
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
        CappedSpanRuleReportCountsEachRecordOnce();
    }
}
