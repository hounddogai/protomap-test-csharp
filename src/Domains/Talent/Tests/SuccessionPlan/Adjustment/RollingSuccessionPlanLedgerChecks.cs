namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Adjustment;

using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingSuccessionPlanLedgerChecks
{
    public static void RollingSuccessionPlanLedgerStartsInCreated()
    {
        var item = RollingSuccessionPlanLedger.NewRollingSuccessionPlanLedger("code-266", "label-367");
        Check.Equal(item.Status, RollingSuccessionPlanLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingSuccessionPlanLedgerMovesToPublished()
    {
        var item = RollingSuccessionPlanLedger.NewRollingSuccessionPlanLedger("code-266", "label-367");
        try
        {
            item.MoveTo(RollingSuccessionPlanLedgerStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingSuccessionPlanLedgerStatus.Published, "status");
    }

    public static void RollingSuccessionPlanLedgerRejectsMovesFromRetired()
    {
        var item = RollingSuccessionPlanLedger.NewRollingSuccessionPlanLedger("code-266", "label-367");
        item.Status = RollingSuccessionPlanLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(RollingSuccessionPlanLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanLedgerStartsInCreated();
        RollingSuccessionPlanLedgerMovesToPublished();
        RollingSuccessionPlanLedgerRejectsMovesFromRetired();
    }
}
