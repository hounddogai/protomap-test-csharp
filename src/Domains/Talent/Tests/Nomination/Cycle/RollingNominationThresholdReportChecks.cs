namespace Acme.Workforce.Talent.Tests.Nomination.Cycle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RollingNominationThresholdReportChecks
{
    public static void RollingNominationThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705254259);
        var items = new List<RollingNominationThreshold>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RollingNominationThreshold.NewRollingNominationThreshold($"rolling-nomination-threshold-{index}", "sample"));
        }
        var report = RollingNominationThresholdReport.BuildRollingNominationThresholdReport("test", items, clock);
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
        RollingNominationThresholdReportCountsEachRecordOnce();
    }
}
