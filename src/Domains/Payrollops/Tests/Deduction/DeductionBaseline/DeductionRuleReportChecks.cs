namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBaseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

/// <summary>Checks of deduction baseline code, run by the project's runner.</summary>
public static class DeductionRuleReportChecks
{
    public static void DeductionRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706697681);
        var items = new List<DeductionRule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DeductionRule.NewDeductionRule($"deduction-rule-{index}", "sample"));
        }
        var report = DeductionRuleReport.BuildDeductionRuleReport("test", items, clock);
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
        DeductionRuleReportCountsEachRecordOnce();
    }
}
