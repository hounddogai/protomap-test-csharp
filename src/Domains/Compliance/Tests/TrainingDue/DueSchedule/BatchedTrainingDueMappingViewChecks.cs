namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class BatchedTrainingDueMappingViewChecks
{
    public static void BatchedTrainingDueMappingViewShowsTheStatusInCapitals()
    {
        var item = BatchedTrainingDueMapping.NewBatchedTrainingDueMapping("code-991", "title-481");
        var view = BatchedTrainingDueMappingView.ToBatchedTrainingDueMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedTrainingDueMappingView.ToBatchedTrainingDueMappingViews(new List<BatchedTrainingDueMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedTrainingDueMappingViewShowsTheStatusInCapitals();
    }
}
