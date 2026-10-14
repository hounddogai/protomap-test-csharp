namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TrainingDueAllocationViewChecks
{
    public static void TrainingDueAllocationViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueAllocation.NewTrainingDueAllocation("code-959", "title-610");
        var view = TrainingDueAllocationView.ToTrainingDueAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueAllocationView.ToTrainingDueAllocationViews(new List<TrainingDueAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueAllocationViewShowsTheStatusInCapitals();
    }
}
