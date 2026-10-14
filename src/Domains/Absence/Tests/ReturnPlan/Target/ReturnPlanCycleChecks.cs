namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanCycleChecks
{
    public static void ReturnPlanCycleStartsInPlanned()
    {
        var item = ReturnPlanCycle.NewReturnPlanCycle("code-170", "title-842");
        Check.Equal(item.Status, ReturnPlanCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanCycleMovesToScheduled()
    {
        var item = ReturnPlanCycle.NewReturnPlanCycle("code-170", "title-842");
        try
        {
            item.MoveTo(ReturnPlanCycleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanCycleStatus.Scheduled, "status");
    }

    public static void ReturnPlanCycleRejectsMovesFromCancelled()
    {
        var item = ReturnPlanCycle.NewReturnPlanCycle("code-170", "title-842");
        item.Status = ReturnPlanCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ReturnPlanCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ReturnPlanCycleStartsInPlanned();
        ReturnPlanCycleMovesToScheduled();
        ReturnPlanCycleRejectsMovesFromCancelled();
    }
}
