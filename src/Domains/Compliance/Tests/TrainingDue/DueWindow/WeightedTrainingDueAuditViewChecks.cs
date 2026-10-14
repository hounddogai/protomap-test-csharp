namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class WeightedTrainingDueAuditViewChecks
{
    public static void WeightedTrainingDueAuditViewShowsTheStatusInCapitals()
    {
        var item = WeightedTrainingDueAudit.NewWeightedTrainingDueAudit("code-46", "title-945");
        var view = WeightedTrainingDueAuditView.ToWeightedTrainingDueAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedTrainingDueAuditView.ToWeightedTrainingDueAuditViews(new List<WeightedTrainingDueAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedTrainingDueAuditViewShowsTheStatusInCapitals();
    }
}
