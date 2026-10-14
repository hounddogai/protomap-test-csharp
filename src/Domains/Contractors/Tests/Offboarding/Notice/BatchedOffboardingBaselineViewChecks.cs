namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedOffboardingBaselineViewChecks
{
    public static void BatchedOffboardingBaselineViewShowsTheStatusInCapitals()
    {
        var item = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline("code-566", "label-210");
        var view = BatchedOffboardingBaselineView.ToBatchedOffboardingBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedOffboardingBaselineView.ToBatchedOffboardingBaselineViews(new List<BatchedOffboardingBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedOffboardingBaselineViewShowsTheStatusInCapitals();
    }
}
