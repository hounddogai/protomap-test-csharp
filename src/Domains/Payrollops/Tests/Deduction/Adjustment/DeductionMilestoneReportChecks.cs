namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeductionMilestoneReportChecks
{
    public static void DeductionMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703414815);
        var items = new List<DeductionMilestone>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DeductionMilestone.NewDeductionMilestone($"deduction-milestone-{index}", "sample"));
        }
        var report = DeductionMilestoneReport.BuildDeductionMilestoneReport("test", items, clock);
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
        DeductionMilestoneReportCountsEachRecordOnce();
    }
}
