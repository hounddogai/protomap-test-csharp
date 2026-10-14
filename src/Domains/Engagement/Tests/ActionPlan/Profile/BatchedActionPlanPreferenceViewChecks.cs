namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class BatchedActionPlanPreferenceViewChecks
{
    public static void BatchedActionPlanPreferenceViewShowsTheStatusInCapitals()
    {
        var item = BatchedActionPlanPreference.NewBatchedActionPlanPreference("code-745", "label-844");
        var view = BatchedActionPlanPreferenceView.ToBatchedActionPlanPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedActionPlanPreferenceView.ToBatchedActionPlanPreferenceViews(new List<BatchedActionPlanPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedActionPlanPreferenceViewShowsTheStatusInCapitals();
    }
}
