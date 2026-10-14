namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class NominationAdjustmentEngineChecks
{
    public static void NominationAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = NominationAdjustmentEngine.DefaultNominationAdjustmentEngine();
        var item = NominationAdjustment.NewNominationAdjustment("code-988", "title-208");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationAdjustmentEngineKeepsResultsWithinBounds();
    }
}
