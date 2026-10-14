namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Agreement;

using Acme.Workforce.Talent.SuccessionPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class SuccessionPlanCycleChecks
{
    public static void SuccessionPlanCycleStartsInNew()
    {
        var item = SuccessionPlanCycle.NewSuccessionPlanCycle("code-221", "label-18");
        Check.Equal(item.Status, SuccessionPlanCycleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanCycleMovesToQueued()
    {
        var item = SuccessionPlanCycle.NewSuccessionPlanCycle("code-221", "label-18");
        try
        {
            item.MoveTo(SuccessionPlanCycleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanCycleStatus.Queued, "status");
    }

    public static void SuccessionPlanCycleRejectsMovesFromFailed()
    {
        var item = SuccessionPlanCycle.NewSuccessionPlanCycle("code-221", "label-18");
        item.Status = SuccessionPlanCycleStatus.Failed;
        Check.Fails(() => item.MoveTo(SuccessionPlanCycleStatus.New), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanCycleStartsInNew();
        SuccessionPlanCycleMovesToQueued();
        SuccessionPlanCycleRejectsMovesFromFailed();
    }
}
