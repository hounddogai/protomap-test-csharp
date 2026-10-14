namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelMetric;

using Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>Checks of model metric code, run by the project's runner.</summary>
public static class CappedCostModelCheckpointChecks
{
    public static void CappedCostModelCheckpointStartsInRequested()
    {
        var item = CappedCostModelCheckpoint.NewCappedCostModelCheckpoint("code-561", "label-223");
        Check.Equal(item.Status, CappedCostModelCheckpointStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedCostModelCheckpointMovesToRevoked()
    {
        var item = CappedCostModelCheckpoint.NewCappedCostModelCheckpoint("code-561", "label-223");
        try
        {
            item.MoveTo(CappedCostModelCheckpointStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedCostModelCheckpointStatus.Revoked, "status");
    }

    public static void CappedCostModelCheckpointRejectsMovesFromExpired()
    {
        var item = CappedCostModelCheckpoint.NewCappedCostModelCheckpoint("code-561", "label-223");
        item.Status = CappedCostModelCheckpointStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedCostModelCheckpointStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedCostModelCheckpointStartsInRequested();
        CappedCostModelCheckpointMovesToRevoked();
        CappedCostModelCheckpointRejectsMovesFromExpired();
    }
}
