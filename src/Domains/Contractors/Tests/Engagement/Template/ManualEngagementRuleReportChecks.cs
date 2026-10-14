namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ManualEngagementRuleReportChecks
{
    public static void ManualEngagementRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707891739);
        var items = new List<ManualEngagementRule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualEngagementRule.NewManualEngagementRule($"manual-engagement-rule-{index}", "sample"));
        }
        var report = ManualEngagementRuleReport.BuildManualEngagementRuleReport("test", items, clock);
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
        ManualEngagementRuleReportCountsEachRecordOnce();
    }
}
