namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultActionPlanManifestChecks
{
    public static void DefaultActionPlanManifestStartsInDraft()
    {
        var item = DefaultActionPlanManifest.NewDefaultActionPlanManifest("code-988", "name-485");
        Check.Equal(item.Status, DefaultActionPlanManifestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultActionPlanManifestMovesToPending()
    {
        var item = DefaultActionPlanManifest.NewDefaultActionPlanManifest("code-988", "name-485");
        try
        {
            item.MoveTo(DefaultActionPlanManifestStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultActionPlanManifestStatus.Pending, "status");
    }

    public static void DefaultActionPlanManifestRejectsMovesFromClosed()
    {
        var item = DefaultActionPlanManifest.NewDefaultActionPlanManifest("code-988", "name-485");
        item.Status = DefaultActionPlanManifestStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultActionPlanManifestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultActionPlanManifestStartsInDraft();
        DefaultActionPlanManifestMovesToPending();
        DefaultActionPlanManifestRejectsMovesFromClosed();
    }
}
