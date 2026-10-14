namespace Acme.Workforce.Compliance.Tests.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredTrainingDueAuditChecks
{
    public static void TieredTrainingDueAuditStartsInPlanned()
    {
        var item = TieredTrainingDueAudit.NewTieredTrainingDueAudit("code-557", "name-559");
        Check.Equal(item.Status, TieredTrainingDueAuditStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredTrainingDueAuditMovesToScheduled()
    {
        var item = TieredTrainingDueAudit.NewTieredTrainingDueAudit("code-557", "name-559");
        try
        {
            item.MoveTo(TieredTrainingDueAuditStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredTrainingDueAuditStatus.Scheduled, "status");
    }

    public static void TieredTrainingDueAuditRejectsMovesFromCancelled()
    {
        var item = TieredTrainingDueAudit.NewTieredTrainingDueAudit("code-557", "name-559");
        item.Status = TieredTrainingDueAuditStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredTrainingDueAuditStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredTrainingDueAuditStartsInPlanned();
        TieredTrainingDueAuditMovesToScheduled();
        TieredTrainingDueAuditRejectsMovesFromCancelled();
    }
}
