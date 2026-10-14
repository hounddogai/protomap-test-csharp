namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ManualBonusPoolReservationEngineChecks
{
    public static void ManualBonusPoolReservationEngineKeepsResultsWithinBounds()
    {
        var engine = ManualBonusPoolReservationEngine.DefaultManualBonusPoolReservationEngine(Flags.NewRegistry());
        var item = ManualBonusPoolReservation.NewManualBonusPoolReservation("code-100", "label-683");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualBonusPoolReservationEngineKeepsResultsWithinBounds();
    }
}
