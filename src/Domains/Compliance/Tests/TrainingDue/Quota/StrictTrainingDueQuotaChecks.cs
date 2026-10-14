namespace Acme.Workforce.Compliance.Tests.TrainingDue.Quota;

using Acme.Workforce.Compliance.TrainingDue.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictTrainingDueQuotaChecks
{
    public static void StrictTrainingDueQuotaStartsInProposed()
    {
        var item = StrictTrainingDueQuota.NewStrictTrainingDueQuota("code-407", "label-589");
        Check.Equal(item.Status, StrictTrainingDueQuotaStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictTrainingDueQuotaMovesToRejected()
    {
        var item = StrictTrainingDueQuota.NewStrictTrainingDueQuota("code-407", "label-589");
        try
        {
            item.MoveTo(StrictTrainingDueQuotaStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictTrainingDueQuotaStatus.Rejected, "status");
    }

    public static void StrictTrainingDueQuotaRejectsMovesFromWithdrawn()
    {
        var item = StrictTrainingDueQuota.NewStrictTrainingDueQuota("code-407", "label-589");
        item.Status = StrictTrainingDueQuotaStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictTrainingDueQuotaStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictTrainingDueQuotaStartsInProposed();
        StrictTrainingDueQuotaMovesToRejected();
        StrictTrainingDueQuotaRejectsMovesFromWithdrawn();
    }
}
