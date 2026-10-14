namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class BatchedActionPlanPreferenceChecks
{
    public static void BatchedActionPlanPreferenceStartsInCreated()
    {
        var item = BatchedActionPlanPreference.NewBatchedActionPlanPreference("code-745", "label-844");
        Check.Equal(item.Status, BatchedActionPlanPreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedActionPlanPreferenceMovesToValidated()
    {
        var item = BatchedActionPlanPreference.NewBatchedActionPlanPreference("code-745", "label-844");
        try
        {
            item.MoveTo(BatchedActionPlanPreferenceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedActionPlanPreferenceStatus.Validated, "status");
    }

    public static void BatchedActionPlanPreferenceRejectsMovesFromRetired()
    {
        var item = BatchedActionPlanPreference.NewBatchedActionPlanPreference("code-745", "label-844");
        item.Status = BatchedActionPlanPreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedActionPlanPreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedActionPlanPreferenceStartsInCreated();
        BatchedActionPlanPreferenceMovesToValidated();
        BatchedActionPlanPreferenceRejectsMovesFromRetired();
    }
}
