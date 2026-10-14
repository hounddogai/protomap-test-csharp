namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultActionPlanDraftChecks
{
    public static void DefaultActionPlanDraftStartsInNew()
    {
        var item = DefaultActionPlanDraft.NewDefaultActionPlanDraft("code-767", "label-759");
        Check.Equal(item.Status, DefaultActionPlanDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultActionPlanDraftMovesToRunning()
    {
        var item = DefaultActionPlanDraft.NewDefaultActionPlanDraft("code-767", "label-759");
        try
        {
            item.MoveTo(DefaultActionPlanDraftStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultActionPlanDraftStatus.Running, "status");
    }

    public static void DefaultActionPlanDraftRejectsMovesFromFailed()
    {
        var item = DefaultActionPlanDraft.NewDefaultActionPlanDraft("code-767", "label-759");
        item.Status = DefaultActionPlanDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultActionPlanDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultActionPlanDraftStartsInNew();
        DefaultActionPlanDraftMovesToRunning();
        DefaultActionPlanDraftRejectsMovesFromFailed();
    }
}
