namespace Acme.Workforce.Talent.Tests.Mobility.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BatchedMobilityReservationEngineChecks
{
    public static void BatchedMobilityReservationEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedMobilityReservationEngine.DefaultBatchedMobilityReservationEngine(Flags.NewRegistry());
        var item = BatchedMobilityReservation.NewBatchedMobilityReservation("code-42", "title-255");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedMobilityReservationEngineKeepsResultsWithinBounds();
    }
}
