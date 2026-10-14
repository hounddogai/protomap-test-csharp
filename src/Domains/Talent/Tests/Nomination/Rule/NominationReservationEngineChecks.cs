namespace Acme.Workforce.Talent.Tests.Nomination.Rule;

using Acme.Workforce.Talent.Nomination.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class NominationReservationEngineChecks
{
    public static void NominationReservationEngineKeepsResultsWithinBounds()
    {
        var engine = NominationReservationEngine.DefaultNominationReservationEngine();
        var item = NominationReservation.NewNominationReservation("code-762", "label-897");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationReservationEngineKeepsResultsWithinBounds();
    }
}
