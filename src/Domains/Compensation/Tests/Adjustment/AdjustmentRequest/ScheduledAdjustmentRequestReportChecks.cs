namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class ScheduledAdjustmentRequestReportChecks
{
    public static void ScheduledAdjustmentRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708449217);
        var items = new List<ScheduledAdjustmentRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest($"scheduled-adjustment-request-{index}", "sample"));
        }
        var report = ScheduledAdjustmentRequestReport.BuildScheduledAdjustmentRequestReport("test", items, clock);
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
        ScheduledAdjustmentRequestReportCountsEachRecordOnce();
    }
}
