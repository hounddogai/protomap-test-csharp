namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ReorgAssignmentReportChecks
{
    public static void ReorgAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707560543);
        var items = new List<ReorgAssignment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ReorgAssignment.NewReorgAssignment($"reorg-assignment-{index}", "sample"));
        }
        var report = ReorgAssignmentReport.BuildReorgAssignmentReport("test", items, clock);
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
        ReorgAssignmentReportCountsEachRecordOnce();
    }
}
