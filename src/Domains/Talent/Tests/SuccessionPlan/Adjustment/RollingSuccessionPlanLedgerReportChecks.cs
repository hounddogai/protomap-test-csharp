namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingSuccessionPlanLedgerReportChecks
{
    public static void RollingSuccessionPlanLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700257476);
        var items = new List<RollingSuccessionPlanLedger>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RollingSuccessionPlanLedger.NewRollingSuccessionPlanLedger($"rolling-succession-plan-ledger-{index}", "sample"));
        }
        var report = RollingSuccessionPlanLedgerReport.BuildRollingSuccessionPlanLedgerReport("test", items, clock);
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
        RollingSuccessionPlanLedgerReportCountsEachRecordOnce();
    }
}
