namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class WeightedBalanceSnapshotEngineChecks
{
    public static void WeightedBalanceSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedBalanceSnapshotEngine.DefaultWeightedBalanceSnapshotEngine(Flags.NewRegistry());
        var item = WeightedBalanceSnapshot.NewWeightedBalanceSnapshot("code-519", "label-845");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedBalanceSnapshotEngineKeepsResultsWithinBounds();
    }
}
