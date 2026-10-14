namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Adjustment;

using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingSuccessionPlanLedgerViewChecks
{
    public static void RollingSuccessionPlanLedgerViewShowsTheStatusInCapitals()
    {
        var item = RollingSuccessionPlanLedger.NewRollingSuccessionPlanLedger("code-266", "label-367");
        var view = RollingSuccessionPlanLedgerView.ToRollingSuccessionPlanLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingSuccessionPlanLedgerView.ToRollingSuccessionPlanLedgerViews(new List<RollingSuccessionPlanLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanLedgerViewShowsTheStatusInCapitals();
    }
}
