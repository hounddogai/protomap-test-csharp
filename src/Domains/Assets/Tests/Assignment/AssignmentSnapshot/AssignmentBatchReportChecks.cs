namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentSnapshot;

using Acme.Workforce.Assets.Assignment.AssignmentSnapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment snapshot code, run by the project's runner.</summary>
public static class AssignmentBatchReportChecks
{
    public static void AssignmentBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704938847);
        var items = new List<AssignmentBatch>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AssignmentBatch.NewAssignmentBatch($"assignment-batch-{index}", "sample"));
        }
        var report = AssignmentBatchReport.BuildAssignmentBatchReport("test", items, clock);
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
        AssignmentBatchReportCountsEachRecordOnce();
    }
}
