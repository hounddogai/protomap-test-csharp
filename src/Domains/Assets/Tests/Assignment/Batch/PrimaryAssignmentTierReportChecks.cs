namespace Acme.Workforce.Assets.Tests.Assignment.Batch;

using Acme.Workforce.Assets.Assignment.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryAssignmentTierReportChecks
{
    public static void PrimaryAssignmentTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709063244);
        var items = new List<PrimaryAssignmentTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PrimaryAssignmentTier.NewPrimaryAssignmentTier($"primary-assignment-tier-{index}", "sample"));
        }
        var report = PrimaryAssignmentTierReport.BuildPrimaryAssignmentTierReport("test", items, clock);
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
        PrimaryAssignmentTierReportCountsEachRecordOnce();
    }
}
