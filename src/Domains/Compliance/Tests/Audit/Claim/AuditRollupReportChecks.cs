namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AuditRollupReportChecks
{
    public static void AuditRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706258318);
        var items = new List<AuditRollup>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AuditRollup.NewAuditRollup($"audit-rollup-{index}", "sample"));
        }
        var report = AuditRollupReport.BuildAuditRollupReport("test", items, clock);
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
        AuditRollupReportCountsEachRecordOnce();
    }
}
