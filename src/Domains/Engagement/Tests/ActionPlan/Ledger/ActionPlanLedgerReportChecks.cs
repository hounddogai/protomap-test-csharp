namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ActionPlanLedgerReportChecks
{
    public static void ActionPlanLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702133195);
        var items = new List<ActionPlanLedger>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ActionPlanLedger.NewActionPlanLedger($"action-plan-ledger-{index}", "sample"));
        }
        var report = ActionPlanLedgerReport.BuildActionPlanLedgerReport("test", items, clock);
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
        ActionPlanLedgerReportCountsEachRecordOnce();
    }
}
