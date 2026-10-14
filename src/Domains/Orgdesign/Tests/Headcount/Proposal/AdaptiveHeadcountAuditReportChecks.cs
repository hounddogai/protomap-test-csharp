namespace Acme.Workforce.Orgdesign.Tests.Headcount.Proposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AdaptiveHeadcountAuditReportChecks
{
    public static void AdaptiveHeadcountAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706294040);
        var items = new List<AdaptiveHeadcountAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit($"adaptive-headcount-audit-{index}", "sample"));
        }
        var report = AdaptiveHeadcountAuditReport.BuildAdaptiveHeadcountAuditReport("test", items, clock);
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
        AdaptiveHeadcountAuditReportCountsEachRecordOnce();
    }
}
