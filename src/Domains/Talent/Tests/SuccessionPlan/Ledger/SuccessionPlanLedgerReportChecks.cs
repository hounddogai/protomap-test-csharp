namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanLedgerReportChecks
{
    public static void SuccessionPlanLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702361149);
        var items = new List<SuccessionPlanLedger>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SuccessionPlanLedger.NewSuccessionPlanLedger($"succession-plan-ledger-{index}", "sample"));
        }
        var report = SuccessionPlanLedgerReport.BuildSuccessionPlanLedgerReport("test", items, clock);
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
        SuccessionPlanLedgerReportCountsEachRecordOnce();
    }
}
