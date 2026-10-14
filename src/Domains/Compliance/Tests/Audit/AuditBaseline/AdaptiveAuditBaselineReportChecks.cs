namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class AdaptiveAuditBaselineReportChecks
{
    public static void AdaptiveAuditBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704878286);
        var items = new List<AdaptiveAuditBaseline>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveAuditBaseline.NewAdaptiveAuditBaseline($"adaptive-audit-baseline-{index}", "sample"));
        }
        var report = AdaptiveAuditBaselineReport.BuildAdaptiveAuditBaselineReport("test", items, clock);
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
        AdaptiveAuditBaselineReportCountsEachRecordOnce();
    }
}
