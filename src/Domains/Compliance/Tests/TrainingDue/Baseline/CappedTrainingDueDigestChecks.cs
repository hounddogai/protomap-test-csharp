namespace Acme.Workforce.Compliance.Tests.TrainingDue.Baseline;

using Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class CappedTrainingDueDigestChecks
{
    public static void CappedTrainingDueDigestStartsInProposed()
    {
        var item = CappedTrainingDueDigest.NewCappedTrainingDueDigest("code-974", "name-285");
        Check.Equal(item.Status, CappedTrainingDueDigestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedTrainingDueDigestMovesToRejected()
    {
        var item = CappedTrainingDueDigest.NewCappedTrainingDueDigest("code-974", "name-285");
        try
        {
            item.MoveTo(CappedTrainingDueDigestStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedTrainingDueDigestStatus.Rejected, "status");
    }

    public static void CappedTrainingDueDigestRejectsMovesFromWithdrawn()
    {
        var item = CappedTrainingDueDigest.NewCappedTrainingDueDigest("code-974", "name-285");
        item.Status = CappedTrainingDueDigestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedTrainingDueDigestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedTrainingDueDigestStartsInProposed();
        CappedTrainingDueDigestMovesToRejected();
        CappedTrainingDueDigestRejectsMovesFromWithdrawn();
    }
}
