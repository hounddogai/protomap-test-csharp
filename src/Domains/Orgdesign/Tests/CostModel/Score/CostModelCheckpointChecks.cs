namespace Acme.Workforce.Orgdesign.Tests.CostModel.Score;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CostModelCheckpointChecks
{
    public static void CostModelCheckpointStartsInRequested()
    {
        var item = CostModelCheckpoint.NewCostModelCheckpoint("code-308", "name-322");
        Check.Equal(item.Status, CostModelCheckpointStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelCheckpointMovesToGranted()
    {
        var item = CostModelCheckpoint.NewCostModelCheckpoint("code-308", "name-322");
        try
        {
            item.MoveTo(CostModelCheckpointStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelCheckpointStatus.Granted, "status");
    }

    public static void CostModelCheckpointRejectsMovesFromExpired()
    {
        var item = CostModelCheckpoint.NewCostModelCheckpoint("code-308", "name-322");
        item.Status = CostModelCheckpointStatus.Expired;
        Check.Fails(() => item.MoveTo(CostModelCheckpointStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CostModelCheckpointStartsInRequested();
        CostModelCheckpointMovesToGranted();
        CostModelCheckpointRejectsMovesFromExpired();
    }
}
