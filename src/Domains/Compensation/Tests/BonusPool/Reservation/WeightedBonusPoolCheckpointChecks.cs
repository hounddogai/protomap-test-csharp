namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedBonusPoolCheckpointChecks
{
    public static void WeightedBonusPoolCheckpointStartsInProposed()
    {
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("code-238", "label-674");
        Check.Equal(item.Status, WeightedBonusPoolCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBonusPoolCheckpointMovesToApproved()
    {
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("code-238", "label-674");
        try
        {
            item.MoveTo(WeightedBonusPoolCheckpointStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBonusPoolCheckpointStatus.Approved, "status");
    }

    public static void WeightedBonusPoolCheckpointRejectsMovesFromWithdrawn()
    {
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("code-238", "label-674");
        item.Status = WeightedBonusPoolCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedBonusPoolCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedBonusPoolCheckpointStartsInProposed();
        WeightedBonusPoolCheckpointMovesToApproved();
        WeightedBonusPoolCheckpointRejectsMovesFromWithdrawn();
    }
}
