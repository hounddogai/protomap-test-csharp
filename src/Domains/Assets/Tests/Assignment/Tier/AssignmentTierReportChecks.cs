namespace Acme.Workforce.Assets.Tests.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AssignmentTierReportChecks
{
    public static void AssignmentTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701291718);
        var items = new List<AssignmentTier>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AssignmentTier.NewAssignmentTier($"assignment-tier-{index}", "sample"));
        }
        var report = AssignmentTierReport.BuildAssignmentTierReport("test", items, clock);
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
        AssignmentTierReportCountsEachRecordOnce();
    }
}
