namespace Acme.Workforce.Compensation.Tests.BonusPool.Profile;

using Acme.Workforce.Compensation.BonusPool.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DefaultBonusPoolReservationEngineChecks
{
    public static void DefaultBonusPoolReservationEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultBonusPoolReservationEngine.DefaultDefaultBonusPoolReservationEngine(Flags.NewRegistry());
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("code-458", "name-11");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultBonusPoolReservationEngineKeepsResultsWithinBounds();
    }
}
