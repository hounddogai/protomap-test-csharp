namespace Acme.Workforce.Compensation.Tests.MeritCycle.Proposal;

using Acme.Workforce.Compensation.MeritCycle.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class MeritCycleGroupReportChecks
{
    public static void MeritCycleGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707619326);
        var items = new List<MeritCycleGroup>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(MeritCycleGroup.NewMeritCycleGroup($"merit-cycle-group-{index}", "sample"));
        }
        var report = MeritCycleGroupReport.BuildMeritCycleGroupReport("test", items, clock);
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
        MeritCycleGroupReportCountsEachRecordOnce();
    }
}
