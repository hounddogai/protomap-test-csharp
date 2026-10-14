namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BatchedSpanReservationEngineChecks
{
    public static void BatchedSpanReservationEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedSpanReservationEngine.DefaultBatchedSpanReservationEngine();
        var item = BatchedSpanReservation.NewBatchedSpanReservation("code-597", "name-319");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedSpanReservationEngineKeepsResultsWithinBounds();
    }
}
