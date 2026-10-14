namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TieredPayBandScheduleEngineChecks
{
    public static void TieredPayBandScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPayBandScheduleEngine.DefaultTieredPayBandScheduleEngine();
        var item = TieredPayBandSchedule.NewTieredPayBandSchedule("code-710", "name-550");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPayBandScheduleEngineKeepsResultsWithinBounds();
    }
}
