namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentAllocation;

using Acme.Workforce.Assets.Assignment.AssignmentAllocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment allocation code, run by the project's runner.</summary>
public static class PrimaryAssignmentAllocationReportChecks
{
    public static void PrimaryAssignmentAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702105198);
        var items = new List<PrimaryAssignmentAllocation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PrimaryAssignmentAllocation.NewPrimaryAssignmentAllocation($"primary-assignment-allocation-{index}", "sample"));
        }
        var report = PrimaryAssignmentAllocationReport.BuildPrimaryAssignmentAllocationReport("test", items, clock);
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
        PrimaryAssignmentAllocationReportCountsEachRecordOnce();
    }
}
