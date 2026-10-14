namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AuditGroupReportChecks
{
    public static void AuditGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705887954);
        var items = new List<AuditGroup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AuditGroup.NewAuditGroup($"audit-group-{index}", "sample"));
        }
        var report = AuditGroupReport.BuildAuditGroupReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AuditGroupReportCountsEachRecordOnce();
    }
}
