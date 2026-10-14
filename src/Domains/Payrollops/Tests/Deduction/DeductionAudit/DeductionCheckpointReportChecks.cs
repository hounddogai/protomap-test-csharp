namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class DeductionCheckpointReportChecks
{
    public static void DeductionCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707080845);
        var items = new List<DeductionCheckpoint>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DeductionCheckpoint.NewDeductionCheckpoint($"deduction-checkpoint-{index}", "sample"));
        }
        var report = DeductionCheckpointReport.BuildDeductionCheckpointReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DeductionCheckpointReportCountsEachRecordOnce();
    }
}
