namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class WeightedBalanceSnapshotChecks
{
    public static void WeightedBalanceSnapshotStartsInRequested()
    {
        var item = WeightedBalanceSnapshot.NewWeightedBalanceSnapshot("code-519", "label-845");
        Check.Equal(item.Status, WeightedBalanceSnapshotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBalanceSnapshotMovesToRevoked()
    {
        var item = WeightedBalanceSnapshot.NewWeightedBalanceSnapshot("code-519", "label-845");
        try
        {
            item.MoveTo(WeightedBalanceSnapshotStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBalanceSnapshotStatus.Revoked, "status");
    }

    public static void WeightedBalanceSnapshotRejectsMovesFromExpired()
    {
        var item = WeightedBalanceSnapshot.NewWeightedBalanceSnapshot("code-519", "label-845");
        item.Status = WeightedBalanceSnapshotStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedBalanceSnapshotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedBalanceSnapshotStartsInRequested();
        WeightedBalanceSnapshotMovesToRevoked();
        WeightedBalanceSnapshotRejectsMovesFromExpired();
    }
}
