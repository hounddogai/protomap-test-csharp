namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleMilestoneReportChecks
{
    public static void MeritCycleMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705007721);
        var items = new List<MeritCycleMilestone>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(MeritCycleMilestone.NewMeritCycleMilestone($"merit-cycle-milestone-{index}", "sample"));
        }
        var report = MeritCycleMilestoneReport.BuildMeritCycleMilestoneReport("test", items, clock);
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
        MeritCycleMilestoneReportCountsEachRecordOnce();
    }
}
