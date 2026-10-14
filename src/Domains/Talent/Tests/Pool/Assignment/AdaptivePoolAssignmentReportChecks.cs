namespace Acme.Workforce.Talent.Tests.Pool.Assignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AdaptivePoolAssignmentReportChecks
{
    public static void AdaptivePoolAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708731730);
        var items = new List<AdaptivePoolAssignment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptivePoolAssignment.NewAdaptivePoolAssignment($"adaptive-pool-assignment-{index}", "sample"));
        }
        var report = AdaptivePoolAssignmentReport.BuildAdaptivePoolAssignmentReport("test", items, clock);
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
        AdaptivePoolAssignmentReportCountsEachRecordOnce();
    }
}
