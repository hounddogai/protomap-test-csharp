namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountSchedule;

using Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

/// <summary>Checks of headcount schedule code, run by the project's runner.</summary>
public static class SeasonalHeadcountReservationEngineChecks
{
    public static void SeasonalHeadcountReservationEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalHeadcountReservationEngine.DefaultSeasonalHeadcountReservationEngine();
        var item = SeasonalHeadcountReservation.NewSeasonalHeadcountReservation("code-430", "title-691");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalHeadcountReservationEngineKeepsResultsWithinBounds();
    }
}
