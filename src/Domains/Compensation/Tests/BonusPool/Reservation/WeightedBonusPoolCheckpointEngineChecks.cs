namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedBonusPoolCheckpointEngineChecks
{
    public static void WeightedBonusPoolCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedBonusPoolCheckpointEngine.DefaultWeightedBonusPoolCheckpointEngine(Flags.NewRegistry());
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("code-238", "label-674");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedBonusPoolCheckpointEngineKeepsResultsWithinBounds();
    }
}
