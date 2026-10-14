namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanMappingChecks
{
    public static void ActionPlanMappingStartsInNew()
    {
        var item = ActionPlanMapping.NewActionPlanMapping("code-881", "title-627");
        Check.Equal(item.Status, ActionPlanMappingStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanMappingMovesToDone()
    {
        var item = ActionPlanMapping.NewActionPlanMapping("code-881", "title-627");
        try
        {
            item.MoveTo(ActionPlanMappingStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanMappingStatus.Done, "status");
    }

    public static void ActionPlanMappingRejectsMovesFromFailed()
    {
        var item = ActionPlanMapping.NewActionPlanMapping("code-881", "title-627");
        item.Status = ActionPlanMappingStatus.Failed;
        Check.Fails(() => item.MoveTo(ActionPlanMappingStatus.New), "move");
    }

    public static void RunAll()
    {
        ActionPlanMappingStartsInNew();
        ActionPlanMappingMovesToDone();
        ActionPlanMappingRejectsMovesFromFailed();
    }
}
