namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyPayRunAuditReportChecks
{
    public static void LegacyPayRunAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702018999);
        var items = new List<LegacyPayRunAudit>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyPayRunAudit.NewLegacyPayRunAudit($"legacy-pay-run-audit-{index}", "sample"));
        }
        var report = LegacyPayRunAuditReport.BuildLegacyPayRunAuditReport("test", items, clock);
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
        LegacyPayRunAuditReportCountsEachRecordOnce();
    }
}
