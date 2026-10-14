namespace Acme.Workforce.Compliance.Tests.TrainingDue.Summary;

using Acme.Workforce.Compliance.TrainingDue.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TrainingDueTemplateChecks
{
    public static void TrainingDueTemplateStartsInPlanned()
    {
        var item = TrainingDueTemplate.NewTrainingDueTemplate("code-901", "label-930");
        Check.Equal(item.Status, TrainingDueTemplateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueTemplateMovesToScheduled()
    {
        var item = TrainingDueTemplate.NewTrainingDueTemplate("code-901", "label-930");
        try
        {
            item.MoveTo(TrainingDueTemplateStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueTemplateStatus.Scheduled, "status");
    }

    public static void TrainingDueTemplateRejectsMovesFromCancelled()
    {
        var item = TrainingDueTemplate.NewTrainingDueTemplate("code-901", "label-930");
        item.Status = TrainingDueTemplateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TrainingDueTemplateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TrainingDueTemplateStartsInPlanned();
        TrainingDueTemplateMovesToScheduled();
        TrainingDueTemplateRejectsMovesFromCancelled();
    }
}
