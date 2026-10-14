namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CappedAccrualScoreChecks
{
    public static void CappedAccrualScoreStartsInProposed()
    {
        var item = CappedAccrualScore.NewCappedAccrualScore("code-909", "label-351");
        Check.Equal(item.Status, CappedAccrualScoreStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAccrualScoreMovesToApproved()
    {
        var item = CappedAccrualScore.NewCappedAccrualScore("code-909", "label-351");
        try
        {
            item.MoveTo(CappedAccrualScoreStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAccrualScoreStatus.Approved, "status");
    }

    public static void CappedAccrualScoreRejectsMovesFromWithdrawn()
    {
        var item = CappedAccrualScore.NewCappedAccrualScore("code-909", "label-351");
        item.Status = CappedAccrualScoreStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedAccrualScoreStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedAccrualScoreStartsInProposed();
        CappedAccrualScoreMovesToApproved();
        CappedAccrualScoreRejectsMovesFromWithdrawn();
    }
}
