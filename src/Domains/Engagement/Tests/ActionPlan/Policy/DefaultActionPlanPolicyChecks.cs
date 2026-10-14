namespace Acme.Workforce.Engagement.Tests.ActionPlan.Policy;

using Acme.Workforce.Engagement.ActionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DefaultActionPlanPolicyChecks
{
    public static void DefaultActionPlanPolicyStartsInDraft()
    {
        var item = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy("code-938", "label-25");
        Check.Equal(item.Status, DefaultActionPlanPolicyStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultActionPlanPolicyMovesToActive()
    {
        var item = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy("code-938", "label-25");
        try
        {
            item.MoveTo(DefaultActionPlanPolicyStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultActionPlanPolicyStatus.Active, "status");
    }

    public static void DefaultActionPlanPolicyRejectsMovesFromClosed()
    {
        var item = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy("code-938", "label-25");
        item.Status = DefaultActionPlanPolicyStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultActionPlanPolicyStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultActionPlanPolicyStartsInDraft();
        DefaultActionPlanPolicyMovesToActive();
        DefaultActionPlanPolicyRejectsMovesFromClosed();
    }
}
