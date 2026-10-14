namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class CappedAccessBadgeTemplateReportChecks
{
    public static void CappedAccessBadgeTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705046568);
        var items = new List<CappedAccessBadgeTemplate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CappedAccessBadgeTemplate.NewCappedAccessBadgeTemplate($"capped-access-badge-template-{index}", "sample"));
        }
        var report = CappedAccessBadgeTemplateReport.BuildCappedAccessBadgeTemplateReport("test", items, clock);
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
        CappedAccessBadgeTemplateReportCountsEachRecordOnce();
    }
}
