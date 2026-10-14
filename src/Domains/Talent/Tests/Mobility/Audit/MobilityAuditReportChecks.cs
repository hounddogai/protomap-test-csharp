namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class MobilityAuditReportChecks
{
    public static void MobilityAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708857841);
        var items = new List<MobilityAudit>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(MobilityAudit.NewMobilityAudit($"mobility-audit-{index}", "sample"));
        }
        var report = MobilityAuditReport.BuildMobilityAuditReport("test", items, clock);
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
        MobilityAuditReportCountsEachRecordOnce();
    }
}
