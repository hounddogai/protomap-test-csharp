namespace Acme.Workforce.Compensation.Tests.EquityGrant.Plan;

using Acme.Workforce.Compensation.EquityGrant.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BatchedEquityGrantPreferenceViewChecks
{
    public static void BatchedEquityGrantPreferenceViewShowsTheStatusInCapitals()
    {
        var item = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference("code-665", "name-540");
        var view = BatchedEquityGrantPreferenceView.ToBatchedEquityGrantPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedEquityGrantPreferenceView.ToBatchedEquityGrantPreferenceViews(new List<BatchedEquityGrantPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedEquityGrantPreferenceViewShowsTheStatusInCapitals();
    }
}
