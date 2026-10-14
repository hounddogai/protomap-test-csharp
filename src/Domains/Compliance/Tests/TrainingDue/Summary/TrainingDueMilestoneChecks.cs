namespace Acme.Workforce.Compliance.Tests.TrainingDue.Summary;

using Acme.Workforce.Compliance.TrainingDue.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TrainingDueMilestoneChecks
{
    public static void TrainingDueMilestoneStartsInDraft()
    {
        var item = TrainingDueMilestone.NewTrainingDueMilestone("code-138", "label-206");
        Check.Equal(item.Status, TrainingDueMilestoneStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueMilestoneMovesToClosed()
    {
        var item = TrainingDueMilestone.NewTrainingDueMilestone("code-138", "label-206");
        try
        {
            item.MoveTo(TrainingDueMilestoneStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueMilestoneStatus.Closed, "status");
    }

    public static void TrainingDueMilestoneRejectsMovesFromClosed()
    {
        var item = TrainingDueMilestone.NewTrainingDueMilestone("code-138", "label-206");
        item.Status = TrainingDueMilestoneStatus.Closed;
        Check.Fails(() => item.MoveTo(TrainingDueMilestoneStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TrainingDueMilestoneStartsInDraft();
        TrainingDueMilestoneMovesToClosed();
        TrainingDueMilestoneRejectsMovesFromClosed();
    }
}
