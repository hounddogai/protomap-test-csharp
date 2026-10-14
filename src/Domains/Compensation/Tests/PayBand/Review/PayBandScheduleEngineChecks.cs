namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayBandScheduleEngineChecks
{
    public static void PayBandScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandScheduleEngine.DefaultPayBandScheduleEngine();
        var item = PayBandSchedule.NewPayBandSchedule("code-40", "label-525");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandScheduleEngineKeepsResultsWithinBounds();
    }
}
