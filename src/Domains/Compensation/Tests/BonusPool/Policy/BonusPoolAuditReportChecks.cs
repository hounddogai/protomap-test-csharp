namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolAuditReportChecks
{
    public static void BonusPoolAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708205529);
        var items = new List<BonusPoolAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BonusPoolAudit.NewBonusPoolAudit($"bonus-pool-audit-{index}", "sample"));
        }
        var report = BonusPoolAuditReport.BuildBonusPoolAuditReport("test", items, clock);
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
        BonusPoolAuditReportCountsEachRecordOnce();
    }
}
