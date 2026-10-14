namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictDeductionMilestoneReportChecks
{
    public static void StrictDeductionMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703860019);
        var items = new List<StrictDeductionMilestone>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictDeductionMilestone.NewStrictDeductionMilestone($"strict-deduction-milestone-{index}", "sample"));
        }
        var report = StrictDeductionMilestoneReport.BuildStrictDeductionMilestoneReport("test", items, clock);
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
        StrictDeductionMilestoneReportCountsEachRecordOnce();
    }
}
