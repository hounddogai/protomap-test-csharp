namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeductionTemplateReportChecks
{
    public static void DeductionTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707520497);
        var items = new List<DeductionTemplate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DeductionTemplate.NewDeductionTemplate($"deduction-template-{index}", "sample"));
        }
        var report = DeductionTemplateReport.BuildDeductionTemplateReport("test", items, clock);
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
        DeductionTemplateReportCountsEachRecordOnce();
    }
}
