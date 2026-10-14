namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class BalanceReservationEngineChecks
{
    public static void BalanceReservationEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceReservationEngine.DefaultBalanceReservationEngine(Flags.NewRegistry());
        var item = BalanceReservation.NewBalanceReservation("code-418", "title-341");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceReservationEngineKeepsResultsWithinBounds();
    }
}
