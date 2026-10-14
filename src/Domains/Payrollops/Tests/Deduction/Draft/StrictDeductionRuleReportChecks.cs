namespace Acme.Workforce.Payrollops.Tests.Deduction.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictDeductionRuleReportChecks
{
    public static void StrictDeductionRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703787498);
        var items = new List<StrictDeductionRule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictDeductionRule.NewStrictDeductionRule($"strict-deduction-rule-{index}", "sample"));
        }
        var report = StrictDeductionRuleReport.BuildStrictDeductionRuleReport("test", items, clock);
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
        StrictDeductionRuleReportCountsEachRecordOnce();
    }
}
