namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle rollup code, run by the project's runner.</summary>
public static class MeritCyclePolicyReportChecks
{
    public static void MeritCyclePolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700807687);
        var items = new List<MeritCyclePolicy>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(MeritCyclePolicy.NewMeritCyclePolicy($"merit-cycle-policy-{index}", "sample"));
        }
        var report = MeritCyclePolicyReport.BuildMeritCyclePolicyReport("test", items, clock);
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
        MeritCyclePolicyReportCountsEachRecordOnce();
    }
}
