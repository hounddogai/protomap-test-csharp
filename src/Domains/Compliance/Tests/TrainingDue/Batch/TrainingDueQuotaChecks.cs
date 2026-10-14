namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TrainingDueQuotaChecks
{
    public static void TrainingDueQuotaStartsInProposed()
    {
        var item = TrainingDueQuota.NewTrainingDueQuota("code-867", "label-76");
        Check.Equal(item.Status, TrainingDueQuotaStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueQuotaMovesToApproved()
    {
        var item = TrainingDueQuota.NewTrainingDueQuota("code-867", "label-76");
        try
        {
            item.MoveTo(TrainingDueQuotaStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueQuotaStatus.Approved, "status");
    }

    public static void TrainingDueQuotaRejectsMovesFromWithdrawn()
    {
        var item = TrainingDueQuota.NewTrainingDueQuota("code-867", "label-76");
        item.Status = TrainingDueQuotaStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TrainingDueQuotaStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TrainingDueQuotaStartsInProposed();
        TrainingDueQuotaMovesToApproved();
        TrainingDueQuotaRejectsMovesFromWithdrawn();
    }
}
