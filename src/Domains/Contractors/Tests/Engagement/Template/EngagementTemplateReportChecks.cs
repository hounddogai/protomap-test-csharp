namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class EngagementTemplateReportChecks
{
    public static void EngagementTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700018297);
        var items = new List<EngagementTemplate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(EngagementTemplate.NewEngagementTemplate($"engagement-template-{index}", "sample"));
        }
        var report = EngagementTemplateReport.BuildEngagementTemplateReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        EngagementTemplateReportCountsEachRecordOnce();
    }
}
