namespace Acme.Workforce.Compliance.Tests.TrainingDue.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TrainingDueAssignmentChecks
{
    public static void TrainingDueAssignmentStartsInDraft()
    {
        var item = TrainingDueAssignment.NewTrainingDueAssignment("code-743", "title-790");
        Check.Equal(item.Status, TrainingDueAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueAssignmentMovesToActive()
    {
        var item = TrainingDueAssignment.NewTrainingDueAssignment("code-743", "title-790");
        try
        {
            item.MoveTo(TrainingDueAssignmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueAssignmentStatus.Active, "status");
    }

    public static void TrainingDueAssignmentRejectsMovesFromClosed()
    {
        var item = TrainingDueAssignment.NewTrainingDueAssignment("code-743", "title-790");
        item.Status = TrainingDueAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(TrainingDueAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TrainingDueAssignmentStartsInDraft();
        TrainingDueAssignmentMovesToActive();
        TrainingDueAssignmentRejectsMovesFromClosed();
    }
}
