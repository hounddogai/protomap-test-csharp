namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

/// <summary>Checks of plan agreement code, run by the project's runner.</summary>
public static class CappedSuccessionPlanCycleChecks
{
    public static void CappedSuccessionPlanCycleStartsInRequested()
    {
        var item = CappedSuccessionPlanCycle.NewCappedSuccessionPlanCycle("code-147", "label-99");
        Check.Equal(item.Status, CappedSuccessionPlanCycleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedSuccessionPlanCycleMovesToGranted()
    {
        var item = CappedSuccessionPlanCycle.NewCappedSuccessionPlanCycle("code-147", "label-99");
        try
        {
            item.MoveTo(CappedSuccessionPlanCycleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedSuccessionPlanCycleStatus.Granted, "status");
    }

    public static void CappedSuccessionPlanCycleRejectsMovesFromExpired()
    {
        var item = CappedSuccessionPlanCycle.NewCappedSuccessionPlanCycle("code-147", "label-99");
        item.Status = CappedSuccessionPlanCycleStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedSuccessionPlanCycleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedSuccessionPlanCycleStartsInRequested();
        CappedSuccessionPlanCycleMovesToGranted();
        CappedSuccessionPlanCycleRejectsMovesFromExpired();
    }
}
