namespace Acme.Workforce.Payrollops.Tests.Deduction.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LenientDeductionEntryReportChecks
{
    public static void LenientDeductionEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707437332);
        var items = new List<LenientDeductionEntry>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientDeductionEntry.NewLenientDeductionEntry($"lenient-deduction-entry-{index}", "sample"));
        }
        var report = LenientDeductionEntryReport.BuildLenientDeductionEntryReport("test", items, clock);
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
        LenientDeductionEntryReportCountsEachRecordOnce();
    }
}
