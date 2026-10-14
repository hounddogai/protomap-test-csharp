namespace Acme.Workforce.Assets.Tests.Assignment.Exception;

using Acme.Workforce.Assets.Assignment.Exception;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class RegionalAssignmentTierReportChecks
{
    public static void RegionalAssignmentTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701956536);
        var items = new List<RegionalAssignmentTier>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RegionalAssignmentTier.NewRegionalAssignmentTier($"regional-assignment-tier-{index}", "sample"));
        }
        var report = RegionalAssignmentTierReport.BuildRegionalAssignmentTierReport("test", items, clock);
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
        RegionalAssignmentTierReportCountsEachRecordOnce();
    }
}
