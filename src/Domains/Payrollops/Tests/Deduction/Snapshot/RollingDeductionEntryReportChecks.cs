namespace Acme.Workforce.Payrollops.Tests.Deduction.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RollingDeductionEntryReportChecks
{
    public static void RollingDeductionEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702095342);
        var items = new List<RollingDeductionEntry>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RollingDeductionEntry.NewRollingDeductionEntry($"rolling-deduction-entry-{index}", "sample"));
        }
        var report = RollingDeductionEntryReport.BuildRollingDeductionEntryReport("test", items, clock);
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
        RollingDeductionEntryReportCountsEachRecordOnce();
    }
}
