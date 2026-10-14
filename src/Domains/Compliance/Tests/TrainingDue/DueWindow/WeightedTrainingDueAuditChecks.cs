namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class WeightedTrainingDueAuditChecks
{
    public static void WeightedTrainingDueAuditStartsInCreated()
    {
        var item = WeightedTrainingDueAudit.NewWeightedTrainingDueAudit("code-46", "title-945");
        Check.Equal(item.Status, WeightedTrainingDueAuditStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedTrainingDueAuditMovesToRetired()
    {
        var item = WeightedTrainingDueAudit.NewWeightedTrainingDueAudit("code-46", "title-945");
        try
        {
            item.MoveTo(WeightedTrainingDueAuditStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedTrainingDueAuditStatus.Retired, "status");
    }

    public static void WeightedTrainingDueAuditRejectsMovesFromRetired()
    {
        var item = WeightedTrainingDueAudit.NewWeightedTrainingDueAudit("code-46", "title-945");
        item.Status = WeightedTrainingDueAuditStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedTrainingDueAuditStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedTrainingDueAuditStartsInCreated();
        WeightedTrainingDueAuditMovesToRetired();
        WeightedTrainingDueAuditRejectsMovesFromRetired();
    }
}
