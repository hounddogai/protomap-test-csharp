namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryDeductionBudgetReportChecks
{
    public static void PrimaryDeductionBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706406295);
        var items = new List<PrimaryDeductionBudget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryDeductionBudget.NewPrimaryDeductionBudget($"primary-deduction-budget-{index}", "sample"));
        }
        var report = PrimaryDeductionBudgetReport.BuildPrimaryDeductionBudgetReport("test", items, clock);
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
        PrimaryDeductionBudgetReportCountsEachRecordOnce();
    }
}
