namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PoolReservationEngineChecks
{
    public static void PoolReservationEngineKeepsResultsWithinBounds()
    {
        var engine = PoolReservationEngine.DefaultPoolReservationEngine(Flags.NewRegistry());
        var item = PoolReservation.NewPoolReservation("code-725", "label-924");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolReservationEngineKeepsResultsWithinBounds();
    }
}
