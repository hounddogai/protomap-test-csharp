namespace Acme.Workforce.Compliance.Tests.TrainingDue.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TrainingDueAssignmentViewChecks
{
    public static void TrainingDueAssignmentViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueAssignment.NewTrainingDueAssignment("code-743", "title-790");
        var view = TrainingDueAssignmentView.ToTrainingDueAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueAssignmentView.ToTrainingDueAssignmentViews(new List<TrainingDueAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueAssignmentViewShowsTheStatusInCapitals();
    }
}
