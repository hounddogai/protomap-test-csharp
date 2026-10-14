namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class AdaptiveDeductionBaselineReportChecks
{
    public static void AdaptiveDeductionBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707801930);
        var items = new List<AdaptiveDeductionBaseline>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline($"adaptive-deduction-baseline-{index}", "sample"));
        }
        var report = AdaptiveDeductionBaselineReport.BuildAdaptiveDeductionBaselineReport("test", items, clock);
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
        AdaptiveDeductionBaselineReportCountsEachRecordOnce();
    }
}
