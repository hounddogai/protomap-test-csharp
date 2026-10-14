namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanAllocationChecks
{
    public static void ActionPlanAllocationStartsInCreated()
    {
        var item = ActionPlanAllocation.NewActionPlanAllocation("code-546", "label-956");
        Check.Equal(item.Status, ActionPlanAllocationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanAllocationMovesToValidated()
    {
        var item = ActionPlanAllocation.NewActionPlanAllocation("code-546", "label-956");
        try
        {
            item.MoveTo(ActionPlanAllocationStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanAllocationStatus.Validated, "status");
    }

    public static void ActionPlanAllocationRejectsMovesFromRetired()
    {
        var item = ActionPlanAllocation.NewActionPlanAllocation("code-546", "label-956");
        item.Status = ActionPlanAllocationStatus.Retired;
        Check.Fails(() => item.MoveTo(ActionPlanAllocationStatus.Created), "move");
    }

    public static void RunAll()
    {
        ActionPlanAllocationStartsInCreated();
        ActionPlanAllocationMovesToValidated();
        ActionPlanAllocationRejectsMovesFromRetired();
    }
}
