namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolRequestChecks
{
    public static void BonusPoolRequestStartsInProposed()
    {
        var item = BonusPoolRequest.NewBonusPoolRequest("code-557", "title-986");
        Check.Equal(item.Status, BonusPoolRequestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolRequestMovesToRejected()
    {
        var item = BonusPoolRequest.NewBonusPoolRequest("code-557", "title-986");
        try
        {
            item.MoveTo(BonusPoolRequestStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolRequestStatus.Rejected, "status");
    }

    public static void BonusPoolRequestRejectsMovesFromWithdrawn()
    {
        var item = BonusPoolRequest.NewBonusPoolRequest("code-557", "title-986");
        item.Status = BonusPoolRequestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BonusPoolRequestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BonusPoolRequestStartsInProposed();
        BonusPoolRequestMovesToRejected();
        BonusPoolRequestRejectsMovesFromWithdrawn();
    }
}
