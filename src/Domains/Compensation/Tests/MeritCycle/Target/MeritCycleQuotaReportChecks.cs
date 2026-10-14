namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleQuotaReportChecks
{
    public static void MeritCycleQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701638360);
        var items = new List<MeritCycleQuota>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(MeritCycleQuota.NewMeritCycleQuota($"merit-cycle-quota-{index}", "sample"));
        }
        var report = MeritCycleQuotaReport.BuildMeritCycleQuotaReport("test", items, clock);
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
        MeritCycleQuotaReportCountsEachRecordOnce();
    }
}
