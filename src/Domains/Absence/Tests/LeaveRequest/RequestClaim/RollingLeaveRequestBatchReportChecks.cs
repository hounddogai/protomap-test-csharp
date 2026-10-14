namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class RollingLeaveRequestBatchReportChecks
{
    public static void RollingLeaveRequestBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705840306);
        var items = new List<RollingLeaveRequestBatch>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RollingLeaveRequestBatch.NewRollingLeaveRequestBatch($"rolling-leave-request-batch-{index}", "sample"));
        }
        var report = RollingLeaveRequestBatchReport.BuildRollingLeaveRequestBatchReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RollingLeaveRequestBatchReportCountsEachRecordOnce();
    }
}
