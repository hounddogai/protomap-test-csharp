namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DeductionOverrideReportChecks
{
    public static void DeductionOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707419678);
        var items = new List<DeductionOverride>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DeductionOverride.NewDeductionOverride($"deduction-override-{index}", "sample"));
        }
        var report = DeductionOverrideReport.BuildDeductionOverrideReport("test", items, clock);
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
        DeductionOverrideReportCountsEachRecordOnce();
    }
}
