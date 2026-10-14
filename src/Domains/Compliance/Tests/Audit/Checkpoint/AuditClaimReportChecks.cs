namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AuditClaimReportChecks
{
    public static void AuditClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703189080);
        var items = new List<AuditClaim>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AuditClaim.NewAuditClaim($"audit-claim-{index}", "sample"));
        }
        var report = AuditClaimReport.BuildAuditClaimReport("test", items, clock);
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
        AuditClaimReportCountsEachRecordOnce();
    }
}
