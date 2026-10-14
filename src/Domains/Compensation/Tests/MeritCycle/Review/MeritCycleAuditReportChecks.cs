namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class MeritCycleAuditReportChecks
{
    public static void MeritCycleAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707430744);
        var items = new List<MeritCycleAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(MeritCycleAudit.NewMeritCycleAudit($"merit-cycle-audit-{index}", "sample"));
        }
        var report = MeritCycleAuditReport.BuildMeritCycleAuditReport("test", items, clock);
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
        MeritCycleAuditReportCountsEachRecordOnce();
    }
}
