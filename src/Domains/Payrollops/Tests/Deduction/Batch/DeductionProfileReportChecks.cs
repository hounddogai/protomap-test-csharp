namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DeductionProfileReportChecks
{
    public static void DeductionProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700835921);
        var items = new List<DeductionProfile>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DeductionProfile.NewDeductionProfile($"deduction-profile-{index}", "sample"));
        }
        var report = DeductionProfileReport.BuildDeductionProfileReport("test", items, clock);
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
        DeductionProfileReportCountsEachRecordOnce();
    }
}
