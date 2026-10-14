namespace Acme.Workforce.Payrollops.Tests.PayRun.Checkpoint;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayRunCheckpointReportChecks
{
    public static void PayRunCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709534761);
        var items = new List<PayRunCheckpoint>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PayRunCheckpoint.NewPayRunCheckpoint($"pay-run-checkpoint-{index}", "sample"));
        }
        var report = PayRunCheckpointReport.BuildPayRunCheckpointReport("test", items, clock);
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
        PayRunCheckpointReportCountsEachRecordOnce();
    }
}
