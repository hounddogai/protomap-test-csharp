namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleClaimReportChecks
{
    public static void MeritCycleClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706449989);
        var items = new List<MeritCycleClaim>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(MeritCycleClaim.NewMeritCycleClaim($"merit-cycle-claim-{index}", "sample"));
        }
        var report = MeritCycleClaimReport.BuildMeritCycleClaimReport("test", items, clock);
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
        MeritCycleClaimReportCountsEachRecordOnce();
    }
}
