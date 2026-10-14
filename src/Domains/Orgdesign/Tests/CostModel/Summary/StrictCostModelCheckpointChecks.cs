namespace Acme.Workforce.Orgdesign.Tests.CostModel.Summary;

using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StrictCostModelCheckpointChecks
{
    public static void StrictCostModelCheckpointStartsInOpen()
    {
        var item = StrictCostModelCheckpoint.NewStrictCostModelCheckpoint("code-760", "label-801");
        Check.Equal(item.Status, StrictCostModelCheckpointStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictCostModelCheckpointMovesToReopened()
    {
        var item = StrictCostModelCheckpoint.NewStrictCostModelCheckpoint("code-760", "label-801");
        try
        {
            item.MoveTo(StrictCostModelCheckpointStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictCostModelCheckpointStatus.Reopened, "status");
    }

    public static void StrictCostModelCheckpointRejectsMovesFromArchived()
    {
        var item = StrictCostModelCheckpoint.NewStrictCostModelCheckpoint("code-760", "label-801");
        item.Status = StrictCostModelCheckpointStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictCostModelCheckpointStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictCostModelCheckpointStartsInOpen();
        StrictCostModelCheckpointMovesToReopened();
        StrictCostModelCheckpointRejectsMovesFromArchived();
    }
}
