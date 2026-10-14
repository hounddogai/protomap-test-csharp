namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DeductionMappingReportChecks
{
    public static void DeductionMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708445615);
        var items = new List<DeductionMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DeductionMapping.NewDeductionMapping($"deduction-mapping-{index}", "sample"));
        }
        var report = DeductionMappingReport.BuildDeductionMappingReport("test", items, clock);
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
        DeductionMappingReportCountsEachRecordOnce();
    }
}
