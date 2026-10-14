namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedBalanceRuleReportChecks
{
    public static void BatchedBalanceRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704383751);
        var items = new List<BatchedBalanceRule>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BatchedBalanceRule.NewBatchedBalanceRule($"batched-balance-rule-{index}", "sample"));
        }
        var report = BatchedBalanceRuleReport.BuildBatchedBalanceRuleReport("test", items, clock);
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
        BatchedBalanceRuleReportCountsEachRecordOnce();
    }
}
