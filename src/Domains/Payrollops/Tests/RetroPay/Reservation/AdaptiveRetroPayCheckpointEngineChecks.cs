namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveRetroPayCheckpointEngineChecks
{
    public static void AdaptiveRetroPayCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveRetroPayCheckpointEngine.DefaultAdaptiveRetroPayCheckpointEngine(Flags.NewRegistry());
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("code-194", "name-767");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveRetroPayCheckpointEngineKeepsResultsWithinBounds();
    }
}
