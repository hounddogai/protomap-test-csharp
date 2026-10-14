namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class BatchedMobilityAllocationEngineChecks
{
    public static void BatchedMobilityAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedMobilityAllocationEngine.DefaultBatchedMobilityAllocationEngine(Flags.NewRegistry());
        var item = BatchedMobilityAllocation.NewBatchedMobilityAllocation("code-805", "title-322");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedMobilityAllocationEngineKeepsResultsWithinBounds();
    }
}
