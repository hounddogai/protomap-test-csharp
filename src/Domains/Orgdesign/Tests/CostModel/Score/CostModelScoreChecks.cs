namespace Acme.Workforce.Orgdesign.Tests.CostModel.Score;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CostModelScoreChecks
{
    public static void CostModelScoreStartsInProposed()
    {
        var item = CostModelScore.NewCostModelScore("code-867", "title-688");
        Check.Equal(item.Status, CostModelScoreStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelScoreMovesToRejected()
    {
        var item = CostModelScore.NewCostModelScore("code-867", "title-688");
        try
        {
            item.MoveTo(CostModelScoreStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelScoreStatus.Rejected, "status");
    }

    public static void CostModelScoreRejectsMovesFromWithdrawn()
    {
        var item = CostModelScore.NewCostModelScore("code-867", "title-688");
        item.Status = CostModelScoreStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CostModelScoreStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CostModelScoreStartsInProposed();
        CostModelScoreMovesToRejected();
        CostModelScoreRejectsMovesFromWithdrawn();
    }
}
