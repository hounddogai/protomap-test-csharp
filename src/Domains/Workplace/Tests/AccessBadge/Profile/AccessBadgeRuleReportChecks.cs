namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AccessBadgeRuleReportChecks
{
    public static void AccessBadgeRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701753248);
        var items = new List<AccessBadgeRule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AccessBadgeRule.NewAccessBadgeRule($"access-badge-rule-{index}", "sample"));
        }
        var report = AccessBadgeRuleReport.BuildAccessBadgeRuleReport("test", items, clock);
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
        AccessBadgeRuleReportCountsEachRecordOnce();
    }
}
