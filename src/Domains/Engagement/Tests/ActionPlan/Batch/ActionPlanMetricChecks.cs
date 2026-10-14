namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ActionPlanMetricChecks
{
    public static void ActionPlanMetricStartsInCreated()
    {
        var item = ActionPlanMetric.NewActionPlanMetric("code-534", "title-933");
        Check.Equal(item.Status, ActionPlanMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanMetricMovesToPublished()
    {
        var item = ActionPlanMetric.NewActionPlanMetric("code-534", "title-933");
        try
        {
            item.MoveTo(ActionPlanMetricStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanMetricStatus.Published, "status");
    }

    public static void ActionPlanMetricRejectsMovesFromRetired()
    {
        var item = ActionPlanMetric.NewActionPlanMetric("code-534", "title-933");
        item.Status = ActionPlanMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(ActionPlanMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        ActionPlanMetricStartsInCreated();
        ActionPlanMetricMovesToPublished();
        ActionPlanMetricRejectsMovesFromRetired();
    }
}
