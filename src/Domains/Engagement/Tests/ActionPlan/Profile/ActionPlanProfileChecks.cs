namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ActionPlanProfileChecks
{
    public static void ActionPlanProfileStartsInOpen()
    {
        var item = ActionPlanProfile.NewActionPlanProfile("code-844", "title-493");
        Check.Equal(item.Status, ActionPlanProfileStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanProfileMovesToInReview()
    {
        var item = ActionPlanProfile.NewActionPlanProfile("code-844", "title-493");
        try
        {
            item.MoveTo(ActionPlanProfileStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanProfileStatus.InReview, "status");
    }

    public static void ActionPlanProfileRejectsMovesFromArchived()
    {
        var item = ActionPlanProfile.NewActionPlanProfile("code-844", "title-493");
        item.Status = ActionPlanProfileStatus.Archived;
        Check.Fails(() => item.MoveTo(ActionPlanProfileStatus.Open), "move");
    }

    public static void RunAll()
    {
        ActionPlanProfileStartsInOpen();
        ActionPlanProfileMovesToInReview();
        ActionPlanProfileRejectsMovesFromArchived();
    }
}
