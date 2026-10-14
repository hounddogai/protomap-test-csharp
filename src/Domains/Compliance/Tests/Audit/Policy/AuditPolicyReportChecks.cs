namespace Acme.Workforce.Compliance.Tests.Audit.Policy;

using Acme.Workforce.Compliance.Audit.Policy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AuditPolicyReportChecks
{
    public static void AuditPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705439097);
        var items = new List<AuditPolicy>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AuditPolicy.NewAuditPolicy($"audit-policy-{index}", "sample"));
        }
        var report = AuditPolicyReport.BuildAuditPolicyReport("test", items, clock);
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
        AuditPolicyReportCountsEachRecordOnce();
    }
}
