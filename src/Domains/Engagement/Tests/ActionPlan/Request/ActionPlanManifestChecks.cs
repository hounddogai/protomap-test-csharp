namespace Acme.Workforce.Engagement.Tests.ActionPlan.Request;

using Acme.Workforce.Engagement.ActionPlan.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ActionPlanManifestChecks
{
    public static void ActionPlanManifestStartsInNew()
    {
        var item = ActionPlanManifest.NewActionPlanManifest("code-535", "title-802");
        Check.Equal(item.Status, ActionPlanManifestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanManifestMovesToFailed()
    {
        var item = ActionPlanManifest.NewActionPlanManifest("code-535", "title-802");
        try
        {
            item.MoveTo(ActionPlanManifestStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanManifestStatus.Failed, "status");
    }

    public static void ActionPlanManifestRejectsMovesFromFailed()
    {
        var item = ActionPlanManifest.NewActionPlanManifest("code-535", "title-802");
        item.Status = ActionPlanManifestStatus.Failed;
        Check.Fails(() => item.MoveTo(ActionPlanManifestStatus.New), "move");
    }

    public static void RunAll()
    {
        ActionPlanManifestStartsInNew();
        ActionPlanManifestMovesToFailed();
        ActionPlanManifestRejectsMovesFromFailed();
    }
}
