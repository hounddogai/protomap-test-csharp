namespace Acme.Workforce.Compliance.Tests.Audit.Tier;

using Acme.Workforce.Compliance.Audit.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AuditTierReportChecks
{
    public static void AuditTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702844750);
        var items = new List<AuditTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AuditTier.NewAuditTier($"audit-tier-{index}", "sample"));
        }
        var report = AuditTierReport.BuildAuditTierReport("test", items, clock);
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
        AuditTierReportCountsEachRecordOnce();
    }
}
