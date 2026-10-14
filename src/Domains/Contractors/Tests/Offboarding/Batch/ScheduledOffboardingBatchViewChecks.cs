namespace Acme.Workforce.Contractors.Tests.Offboarding.Batch;

using Acme.Workforce.Contractors.Offboarding.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ScheduledOffboardingBatchViewChecks
{
    public static void ScheduledOffboardingBatchViewShowsTheStatusInCapitals()
    {
        var item = ScheduledOffboardingBatch.NewScheduledOffboardingBatch("code-893", "name-823");
        var view = ScheduledOffboardingBatchView.ToScheduledOffboardingBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledOffboardingBatchView.ToScheduledOffboardingBatchViews(new List<ScheduledOffboardingBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledOffboardingBatchViewShowsTheStatusInCapitals();
    }
}
