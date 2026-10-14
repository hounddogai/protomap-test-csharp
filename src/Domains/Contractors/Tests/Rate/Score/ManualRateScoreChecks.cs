namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ManualRateScoreChecks
{
    public static void ManualRateScoreStartsInProposed()
    {
        var item = ManualRateScore.NewManualRateScore("code-861", "name-357");
        Check.Equal(item.Status, ManualRateScoreStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualRateScoreMovesToRejected()
    {
        var item = ManualRateScore.NewManualRateScore("code-861", "name-357");
        try
        {
            item.MoveTo(ManualRateScoreStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualRateScoreStatus.Rejected, "status");
    }

    public static void ManualRateScoreRejectsMovesFromWithdrawn()
    {
        var item = ManualRateScore.NewManualRateScore("code-861", "name-357");
        item.Status = ManualRateScoreStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualRateScoreStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualRateScoreStartsInProposed();
        ManualRateScoreMovesToRejected();
        ManualRateScoreRejectsMovesFromWithdrawn();
    }
}
